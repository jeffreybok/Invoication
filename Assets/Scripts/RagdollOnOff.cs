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