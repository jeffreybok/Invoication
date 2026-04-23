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

        Vector3 pos = transform.position;


        // ✅ SAME PATTERN AS BOX (VFX via RPC)
        PlayExplosion_ObserversRPC(pos);

        Collider[] hitColliders = Physics.OverlapSphere(pos, explosionRadius);

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
                ApplyForce_ObserversRPC(rb.gameObject, pos, explosionRadius);
        }

        Destroy(gameObject);
    }

    [ObserversRpc]
    void PlayExplosion_ObserversRPC(Vector3 pos)
    {
        // ❌ REMOVED custom audio (THIS WAS YOUR BUG)

        // ✅ ONLY VFX (like box)
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, pos, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            Destroy(explosion, ps != null ? ps.main.duration : 2f);
        }

        // camera shake still fine
        CameraShake.Instance?.ShakeFromPosition(pos, 20f);
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