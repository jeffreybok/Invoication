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
    private GameObject attacker;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        if (Time.time - spawnTime < 0.5f) return; // ignore collisions on spawn

        bool shouldExplode = false;
        GameObject detectedAttacker = null;

        if (explodeFromPickup && collision.gameObject.CompareTag("Pickup"))
        {
            shouldExplode = true;
            detectedAttacker = collision.gameObject;
        }

        if (explodeFromFireball && collision.gameObject.GetComponent<Fireball>() != null)
        {
            shouldExplode = true;
            detectedAttacker = collision.gameObject;
        }

        if (explodeOnImpact && collision.relativeVelocity.magnitude > impactThreshold)
        {
            shouldExplode = true;
            detectedAttacker = collision.gameObject;
        }

        if (!shouldExplode) return;

#if UNITY_EDITOR
        ExplodeInternal(detectedAttacker);
#else
        if (isServer)
            ExplodeInternal(detectedAttacker);
        else
            RequestExplosion_ServerRPC(detectedAttacker);
#endif
    }

    public void TriggerExplosion(GameObject attackerObj = null)
    {
        if (hasExploded) return;

#if UNITY_EDITOR
        ExplodeInternal(attackerObj);
#else
        if (isServer)
            ExplodeInternal(attackerObj);
        else
            RequestExplosion_ServerRPC(attackerObj);
#endif
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
        
        SoundManager.Instance.PlayExplosion(transform.position);

#if UNITY_EDITOR
        SpawnExplosionVFX(transform.position);
#else
        PlayExplosion_ObserversRPC(transform.position);
#endif

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage, attacker);
                enemy.HitByExplosion();
            }

            // Damage players in explosion radius (excludes attacker)
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null && playerHealth.gameObject != attacker)
                playerHealth.TakeDamage(explosionDamage);

            ExplosiveObject other = hit.GetComponent<ExplosiveObject>();
            if (other != null && other != this && !other.hasExploded)
                other.TriggerExplosion(attacker);

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
                ApplyForce_ObserversRPC(rb.gameObject, transform.position);
        }

        Destroy(gameObject);
    }

    [ObserversRpc]
    void PlayExplosion_ObserversRPC(Vector3 pos)
    {
        SpawnExplosionVFX(pos);
    }

    void SpawnExplosionVFX(Vector3 pos)
    {
        if (explosionEffect == null) return;

        GameObject explosion = Instantiate(explosionEffect, pos, Quaternion.identity);
        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
        float duration = ps != null ? ps.main.duration : 2f;
        Destroy(explosion, duration);
    }

    [ObserversRpc]
    void ApplyForce_ObserversRPC(GameObject target, Vector3 origin)
    {
        if (target == null) return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
            rb.AddExplosionForce(explosionForce, origin, explosionRadius, 1f, ForceMode.Impulse);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}