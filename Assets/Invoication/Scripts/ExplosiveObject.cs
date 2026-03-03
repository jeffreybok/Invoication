using UnityEngine;

public class ExplosiveObject : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 8f;
    public float explosionDamage = 75f;
    public float explosionForce = 600f;
    public GameObject explosionEffect;
    
    [Header("Trigger Settings")]
    public bool explodeOnImpact = true;
    public float impactThreshold = 5f; // How hard it needs to be hit
    public bool explodeFromFireball = true;
    public bool explodeFromPickup = true; // Thrown items
    
    private bool hasExploded = false;
    
    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        
        bool shouldExplode = false;
        
        // Check for thrown items
        if (explodeFromPickup && collision.gameObject.CompareTag("Pickup"))
        {
            shouldExplode = true;
        }
        
        // Check for fireball
        if (explodeFromFireball && collision.gameObject.GetComponent<Fireball>() != null)
        {
            shouldExplode = true;
        }
        
        // Check impact force
        if (explodeOnImpact && collision.relativeVelocity.magnitude > impactThreshold)
        {
            shouldExplode = true;
        }
        
        if (shouldExplode)
        {
            Explode();
        }
    }
    
    public void TriggerExplosion()
    {
        if (!hasExploded)
        {
            Explode();
        }
    }
    
    void Explode()
    {
        hasExploded = true;
        
        Debug.Log(gameObject.name + " exploded!");
        
        // Spawn explosion effect
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 2f;
            
            Destroy(explosion, duration);
        }
        
        // Find all objects in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hit in hitColliders)
        {
            // Damage enemies
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage);
                enemy.HitByExplosion();
            }
            
            // Chain reaction with other explosive objects
            ExplosiveObject otherExplosive = hit.GetComponent<ExplosiveObject>();
            if (otherExplosive != null && otherExplosive != this && !otherExplosive.hasExploded)
            {
                otherExplosive.Explode();
            }
            
            // Apply force to rigidbodies
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
        }
        
        // Destroy the object
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}