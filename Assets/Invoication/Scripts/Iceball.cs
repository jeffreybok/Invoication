using UnityEngine;

public class Iceball : MonoBehaviour
{
    [Header("Freeze Settings")]
    public float freezeRadius = 2f;
    public float freezeDuration = 3f;
    public GameObject freezeEffect;
    
    void OnCollisionEnter(Collision collision)
    {
        Freeze();
    }
    
    void Freeze()
    {
        Debug.Log("Iceball shattered at: " + transform.position);
    
        // Spawn freeze effect
        if (freezeEffect != null)
        {
            GameObject effect = Instantiate(freezeEffect, transform.position, Quaternion.identity);
            
            // Add FreezeZone component to handle the freezing
            FreezeZone freezeZone = effect.AddComponent<FreezeZone>();
            freezeZone.freezeRadius = freezeRadius;
            freezeZone.freezeDuration = freezeDuration;
            
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 2f;
            
            Destroy(effect, duration);
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, freezeRadius);
    }
}