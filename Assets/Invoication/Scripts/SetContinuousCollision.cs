using UnityEngine;

public class SetContinuousCollision : MonoBehaviour
{
    [ContextMenu("Set All Rigidbodies to Continuous")]
    void SetContinuous()
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Debug.Log(rb.name + " set to Continuous");
        }
        Debug.Log("Done! " + rbs.Length + " Rigidbodies updated.");
    }
}