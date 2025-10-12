using UnityEngine;

public class IncreaseDamping : MonoBehaviour
{
    [ContextMenu("Increase Ragdoll Damping")]
    void ApplyDamping()
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.linearDamping = 150f;    // Slows linear movement
            rb.angularDamping = 150f;   // Slows rotation
            Debug.Log(rb.name + " damping increased");
        }
        Debug.Log("Damping increased on " + rbs.Length + " rigidbodies");
    }
}