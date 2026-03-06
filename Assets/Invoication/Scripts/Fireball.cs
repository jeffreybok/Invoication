using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;

    private GameObject owner;

    // Called immediately after spawning
    public void SetOwner(GameObject shooter)
    {
        owner = shooter;

        Collider myCollider = GetComponent<Collider>();
        Collider[] ownerColliders = shooter.GetComponentsInChildren<Collider>();

        // Ignore ALL colliders on the shooter
        foreach (Collider col in ownerColliders)
        {
            Physics.IgnoreCollision(myCollider, col);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Extra safety — never explode on owner
        if (owner != null && collision.transform.root.gameObject == owner)
            return;

        Explode();
    }

    void Explode()
    {
        Debug.Log("Fireball exploded at: " + transform.position);

        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 2f;

            Destroy(explosion, duration);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            // Ignore owner and anything belonging to owner
            if (owner != null && hit.transform.root.gameObject == owner)
                continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage);
                enemy.HitByExplosion();
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(200f, transform.position, explosionRadius, 0.5f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}