using UnityEngine;

public class DeleteRagdoll : MonoBehaviour
{
    void Start()
    {
        // Get all child components
        CharacterJoint[] joints = GetComponentsInChildren<CharacterJoint>();
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        
        // Delete all character joints
        foreach (CharacterJoint joint in joints)
        {
            DestroyImmediate(joint);
        }
        
        // Delete all child rigidbodies (not the main one)
        foreach (Rigidbody rb in rbs)
        {
            if (rb.transform != transform) // Don't delete main rigidbody
            {
                DestroyImmediate(rb);
            }
        }
        
        Debug.Log("Ragdoll components deleted!");
    }
}