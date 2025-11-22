using UnityEngine;

public class RagdollOnOff : MonoBehaviour
{
    public BoxCollider mainCollider;
    public GameObject EnemyRig;
    public Animator EnemyAnimator;
    
    private Enemy enemyScript;
    
    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        GetRagdollBits();
        RagdollModeOff();
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Pickup")
        {
            // Check if the pickup was moving fast enough (i.e., was thrown)
            Rigidbody pickupRb = collision.gameObject.GetComponent<Rigidbody>();
            float impactVelocity = pickupRb != null ? pickupRb.linearVelocity.magnitude : 0f;
            
            // Only ragdoll if the item was thrown with enough force
            if (impactVelocity < 3f) return; // Adjust threshold as needed
            
            // Tell enemy it was hit
            if (enemyScript != null)
            {
                enemyScript.OnHitByPickup();
            }
            
            // Only ragdoll if enemy allows it (not frozen)
            if (enemyScript == null || !enemyScript.IsFrozen())
            {
                RagdollModeOn();
            }
        }
    }

    Collider[] ragDollColliders;
    Rigidbody[] limbsRigidbodies;

    void GetRagdollBits()
    {
        ragDollColliders = EnemyRig.GetComponentsInChildren<Collider>();
        limbsRigidbodies = EnemyRig.GetComponentsInChildren<Rigidbody>();
    }

    public void RagdollModeOn()
    {
        EnemyAnimator.enabled = false;
        foreach(Collider col in ragDollColliders)
        {
            col.enabled = true;
        }
        foreach(Rigidbody rb in limbsRigidbodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        mainCollider.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void RagdollModeOff()
    {
        foreach(Collider col in ragDollColliders)
        {
            col.enabled = false;
        }
        foreach(Rigidbody rb in limbsRigidbodies)
        {
            rb.isKinematic= true;
        }

        EnemyAnimator.enabled = true;
        mainCollider.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }
}