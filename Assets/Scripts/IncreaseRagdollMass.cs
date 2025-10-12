using UnityEngine;

public class IncreaseRagdollMass : MonoBehaviour
{
    [Header("Settings")]
    public float massMultiplier = 5f; // Multiply all masses by this amount
    
    [ContextMenu("Increase All Ragdoll Masses")]
    public void IncreaseMasses()
    {
        Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>();
        
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.mass *= massMultiplier;
            Debug.Log(rb.name + " mass increased to: " + rb.mass);
        }
        
        Debug.Log("All ragdoll masses increased by " + massMultiplier + "x");
    }
    
    [ContextMenu("Set All Ragdoll Masses")]
    public void SetMasses()
    {
        Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>();
        
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.mass = massMultiplier;
            Debug.Log(rb.name + " mass set to: " + rb.mass);
        }
        
        Debug.Log("All ragdoll masses set to " + massMultiplier);
    }
}