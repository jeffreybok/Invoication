using UnityEngine;
using PurrNet;

public class Fireball : NetworkBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;

    private GameObject attacker;
    private bool hasExploded = false;

    public void Initialize(GameObject shooter)
    {
        attacker = shooter;

        if (attacker != null)
        {
            Collider myCollider = GetComponent<Collider>();
            Collider[] attackerColliders = attacker.GetComponentsInChildren<Collider>();

            if (myCollider != null)
                foreach (Collider col in attackerColliders)
                    if (col != null)
                        Physics.IgnoreCollision(myCollider, col);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        if (attacker != null && collision.transform.root.gameObject == attacker) return;

        if (isServer) ExplodeInternal();
        else RequestExplode_ServerRPC();
    }

    [ServerRpc]
    void RequestExplode_ServerRPC()
    {
        if (!isServer || hasExploded) return;
        ExplodeInternal();
    }

    void ExplodeInternal()
    {
        if (!isServer || hasExploded) return;
        hasExploded = true;

        PlayExplosion_ObserversRPC(transform.position);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            if (attacker != null && hit.transform.root.gameObject == attacker) continue;

            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage_Server(explosionDamage, attacker);
                enemy.HitByExplosion(attacker);
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
                ApplyForce_ObserversRPC(rb.gameObject, transform.position, explosionRadius);
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
            Destroy(explosion, ps != null ? ps.main.duration : 2f);
        }
    }

    [ObserversRpc]
    void ApplyForce_ObserversRPC(GameObject target, Vector3 origin, float radius)
    {
        if (target == null) return;
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
            rb.AddExplosionForce(200f, origin, radius, 0.5f, ForceMode.Impulse);
    }
}