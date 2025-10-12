using UnityEngine;

public class ConfigureRagdoll : MonoBehaviour
{
    [Header("Settings")]
    public float mass = 25f;
    public float linearDamping = 5f;
    public float angularDamping = 5f;
    public PhysicsMaterial frictionMaterial;
    
    [ContextMenu("Configure All Ragdoll Parts")]
    public void Configure()
    {
        // Configure all Rigidbodies
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.mass = mass;
            rb.linearDamping = linearDamping;
            rb.angularDamping = angularDamping;
            Debug.Log(rb.name + " - Mass: " + mass + ", Damping: " + linearDamping);
        }
        
        // Apply friction to all Colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (frictionMaterial != null)
            {
                col.material = frictionMaterial;
                Debug.Log(col.name + " - Friction applied");
            }
        }
        
        Debug.Log("Configured " + rbs.Length + " rigidbodies and " + colliders.Length + " colliders");
    }
}
