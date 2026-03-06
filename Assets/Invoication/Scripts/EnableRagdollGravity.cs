using UnityEngine;

public class EnableRagdollGravity : MonoBehaviour
{
    [ContextMenu("Enable Gravity on All Rigidbodies")]
    void EnableGravity()
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.useGravity = true;
            Debug.Log(rb.name + " - Gravity enabled");
        }
        Debug.Log("Gravity enabled on " + rbs.Length + " rigidbodies");
    }
}