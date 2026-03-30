using UnityEngine;
using PurrNet;

public class Fireball : NetworkBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;

    // FIX: do NOT conflict with NetworkIdentity.owner
    private GameObject attacker;
    private PlayerStats attackerStats;

    private bool hasExploded = false;

    // =========================
    // INIT (used by SpellCaster)
    // =========================
    public void Initialize(GameObject shooter)
    {
        attacker = shooter;

        if (attacker != null)
        {
            attackerStats = attacker.GetComponent<PlayerStats>();

            Collider myCollider = GetComponent<Collider>();
            Collider[] attackerColliders = attacker.GetComponentsInChildren<Collider>();

            if (myCollider != null)
            {
                foreach (Collider col in attackerColliders)
                {
                    if (col != null)
                        Physics.IgnoreCollision(myCollider, col);
                }
            }
        }
    }

    // =========================
    // COLLISION
    // =========================
    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (attacker != null && collision.transform.root.gameObject == attacker)
            return;

        if (isServer)
        {
            ExplodeInternal();
        }
        else
        {
            RequestExplode_ServerRPC();
        }
    }

    [ServerRpc]
    void RequestExplode_ServerRPC()
    {
        if (!isServer) return;
        if (hasExploded) return;

        ExplodeInternal();
    }

    // =========================
    // EXPLOSION LOGIC
    // =========================
    void ExplodeInternal()
    {
        if (!isServer) return;
        if (hasExploded) return;

        hasExploded = true;

        float damage = attackerStats != null
            ? attackerStats.GetFireballDamage()
            : explosionDamage;

        float radius = attackerStats != null
            ? attackerStats.fireballExplosionRadius
            : explosionRadius;

        // VISUAL SYNC
        PlayExplosion_ObserversRPC(transform.position);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hitColliders)
        {
            if (attacker != null && hit.transform.root.gameObject == attacker)
                continue;

            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                // SERVER AUTHORITATIVE + ATTACKER PASSED
                enemy.TakeDamage_Server(damage, attacker);
                enemy.HitByExplosion(attacker);
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                ApplyForce_ObserversRPC(rb.gameObject, transform.position, radius);
            }
        }

        Destroy(gameObject);
    }

    // =========================
    // VISUAL SYNC
    // =========================
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

    // =========================
    // FORCE SYNC
    // =========================
    [ObserversRpc]
    void ApplyForce_ObserversRPC(GameObject target, Vector3 origin, float radius)
    {
        if (target == null) return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.AddExplosionForce(200f, origin, radius, 0.5f, ForceMode.Impulse);
        }
    }
}