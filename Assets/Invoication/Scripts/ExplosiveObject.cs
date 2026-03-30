using UnityEngine;
using PurrNet;

public class ExplosiveObject : NetworkBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 8f;
    public float explosionDamage = 75f;
    public float explosionForce = 600f;
    public GameObject explosionEffect;
    
    [Header("Trigger Settings")]
    public bool explodeOnImpact = true;
    public float impactThreshold = 5f;
    public bool explodeFromFireball = true;
    public bool explodeFromPickup = true;
    
    private bool hasExploded = false;

    // Track who caused explosion
    private GameObject attacker;

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        bool shouldExplode = false;
        GameObject detectedAttacker = null;

        // Pickup hit
        if (explodeFromPickup && collision.gameObject.CompareTag("Pickup"))
        {
            shouldExplode = true;
            detectedAttacker = collision.gameObject;
        }

        // Fireball hit
        if (explodeFromFireball && collision.gameObject.GetComponent<Fireball>() != null)
        {
            shouldExplode = true;
            detectedAttacker = collision.gameObject;
        }

        // Impact force
        if (explodeOnImpact && collision.relativeVelocity.magnitude > impactThreshold)
        {
            shouldExplode = true;
            detectedAttacker = collision.gameObject;
        }

        if (!shouldExplode) return;

        if (isServer)
        {
            ExplodeInternal(detectedAttacker);
        }
        else
        {
            RequestExplosion_ServerRPC(detectedAttacker);
        }
    }

    public void TriggerExplosion(GameObject attackerObj = null)
    {
        if (hasExploded) return;

        if (isServer)
        {
            ExplodeInternal(attackerObj);
        }
        else
        {
            RequestExplosion_ServerRPC(attackerObj);
        }
    }

    [ServerRpc]
    void RequestExplosion_ServerRPC(GameObject attackerObj)
    {
        if (hasExploded) return;

        ExplodeInternal(attackerObj);
    }

    void ExplodeInternal(GameObject attackerObj)
    {
        if (hasExploded) return;

        hasExploded = true;
        attacker = attackerObj;

        // Sync explosion visuals
        PlayExplosion_ObserversRPC(transform.position);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            // DAMAGE ENEMIES (SERVER ONLY)
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage, attacker);
                enemy.HitByExplosion();
            }

            // CHAIN EXPLOSIONS
            ExplosiveObject other = hit.GetComponent<ExplosiveObject>();
            if (other != null && other != this && !other.hasExploded)
            {
                other.TriggerExplosion(attacker);
            }

            // APPLY FORCE (SYNCED)
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                ApplyForce_ObserversRPC(rb.gameObject, transform.position);
            }
        }

        Destroy(gameObject);
    }

    [ObserversRpc]
    void PlayExplosion_ObserversRPC(Vector3 pos)
    {
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, pos, Quaternion.identity);

            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 2f;

            Destroy(explosion, duration);
        }
    }

    [ObserversRpc]
    void ApplyForce_ObserversRPC(GameObject target, Vector3 origin)
    {
        if (target == null) return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.AddExplosionForce(explosionForce, origin, explosionRadius, 1f, ForceMode.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}