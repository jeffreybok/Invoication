using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;
    
    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }
    
    void Explode()
    {
        Debug.Log("Fireball exploded at: " + transform.position);
    
        // Spawn explosion effect
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 2f;
            
            Destroy(explosion, duration);
        }
        
        // Find all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hit in hitColliders)
        {
            // Damage and ragdoll enemies
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage);
                
                // Tell the enemy it got hit by explosion
                enemy.HitByExplosion();
                
                Debug.Log("Hit enemy: " + enemy.name);
            }
            
            // Apply explosion force to rigidbodies (including ragdoll bones)
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