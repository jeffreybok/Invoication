using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Animator animator;
    private Collider mainCollider;

    void Start()
    {
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        
        DisableRagdoll();
    }

    public void DisableRagdoll()
    {
        // Stop all ragdoll movement first
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.None; // Clear any constraints
        }
        
        foreach (Collider col in ragdollColliders)
        {
            if (col != mainCollider)
            {
                col.enabled = false;
            }
        }
        
        // Re-enable main collider
        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }
        
        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind(); // Reset bones to default pose
            animator.Update(0f);
        }
    }

    public void EnableRagdoll()
    {
        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
            // Remove the constraints line - let physics work normally
        }
        
        foreach (Collider col in ragdollColliders)
        {
            col.enabled = true;
        }
        
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    public Vector3 GetRagdollPosition()
    {
        // Find the lowest point of the ragdoll to place on ground
        float lowestY = float.MaxValue;
        Vector3 lowestPosition = transform.position;
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb.position.y < lowestY)
            {
                lowestY = rb.position.y;
                lowestPosition = rb.position;
            }
        }
        
        // Return the horizontal position of the pelvis, but with the lowest Y
        if (ragdollRigidbodies.Length > 0)
        {
            Vector3 pelvisPos = ragdollRigidbodies[0].position;
            return new Vector3(pelvisPos.x, lowestY, pelvisPos.z);
        }
        
        return lowestPosition;
    }

    public void ResetBonesToBindPose()
    {
        // Force animator to reset to T-pose/bind pose momentarily
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}