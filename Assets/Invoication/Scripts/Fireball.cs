using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;

    private GameObject owner;
    private PlayerStats ownerStats;

    public void SetOwner(GameObject shooter)
    {
        owner = shooter;
        ownerStats = shooter.GetComponent<PlayerStats>();

        Collider myCollider = GetComponent<Collider>();
        Collider[] ownerColliders = shooter.GetComponentsInChildren<Collider>();

        foreach (Collider col in ownerColliders)
        {
            Physics.IgnoreCollision(myCollider, col);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (owner != null && collision.transform.root.gameObject == owner)
            return;

        Explode();
    }

    void Explode()
    {
        Debug.Log("Fireball exploded at: " + transform.position);

        float damage = ownerStats != null
            ? ownerStats.GetFireballDamage()
            : explosionDamage;

        float radius = ownerStats != null
            ? ownerStats.fireballExplosionRadius
            : explosionRadius;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hitColliders)
        {
            if (owner != null && hit.transform.root.gameObject == owner) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                enemy.HitByExplosion();
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
                rb.AddExplosionForce(200f, transform.position, radius, 0.5f, ForceMode.Impulse);
        }

        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            Destroy(explosion, ps != null ? ps.main.duration : 2f);
        }

        Destroy(gameObject);
    }
}