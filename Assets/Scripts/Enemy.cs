using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("References")]
    public Transform player;
    
    [Header("Freeze Settings")]
    private bool isFrozen = false;
    private Renderer[] renderers;
    private Color[][] originalColors;
    
    private Animator animator;
    private RagdollOnOff ragdollOnOff;
    private bool isDead = false;
    private bool isRagdolled = false;
    private Transform hipsBone;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        ragdollOnOff = GetComponent<RagdollOnOff>();
        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        
        // Store renderers and original colors
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length][];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = new Color[renderers[i].materials.Length];
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                originalColors[i][j] = renderers[i].materials[j].color;
            }
        }
        
        if (animator != null)
        {
            animator.Play("Armature|Idle");
        }
    }
    
    void Update()
    {
        if (isFrozen || isDead) return; // Don't do anything while frozen
    }
    
    // void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Pickup"))
    //     {
    //         // Always take damage, even when frozen
    //         TakeDamage(30f);
            
    //         // Only ragdoll if NOT frozen and NOT dead
    //         if (!isFrozen && !isDead)
    //         {
    //             isRagdolled = true;
    //             StartCoroutine(WaitForRagdollToSettle());
    //         }
    //     }
    // }

    public void OnHitByPickup()
    {
        // Always take damage
        TakeDamage(30f);
        
        // If frozen, don't start recovery (will recover when unfrozen)
        // If not frozen and not dead, ragdoll normally
        if (!isFrozen && !isDead)
        {
            isRagdolled = true;
            StartCoroutine(WaitForRagdollToSettle());
        }
        else if (isFrozen && !isDead)
        {
            // Mark as ragdolled so we know to recover after unfreeze
            isRagdolled = true;
        }
    }
    
    System.Collections.IEnumerator WaitForRagdollToSettle()
    {
        yield return new WaitForSeconds(1f);
        
        Rigidbody[] ragdollRbs = GetComponentsInChildren<Rigidbody>();
        
        bool isMoving = true;
        while (isMoving)
        {
            isMoving = false;
            
            foreach (Rigidbody rb in ragdollRbs)
            {
                if (rb.linearVelocity.magnitude > 0.1f || rb.angularVelocity.magnitude > 0.1f)
                {
                    isMoving = true;
                    break;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (!isDead && !isFrozen)
        {
            RecoverFromRagdoll();
        }
    }
    
    void RecoverFromRagdoll()
    {
        if (ragdollOnOff != null)
        {
            ragdollOnOff.RagdollModeOff();
        }
        
        isRagdolled = false;
        
        AlignPositionToHips();
        
        // Keep Y rotation, reset X and Z to stand upright
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        
        if (animator != null)
        {
            animator.Play("Armature|Idle");
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        isRagdolled = true;
        
        // Cancel freeze and restore animator if frozen
        if (isFrozen)
        {
            CancelInvoke(nameof(Unfreeze));
            isFrozen = false;
            
            // Re-enable animator before ragdolling
            if (animator != null)
            {
                animator.enabled = true;
            }
            
            // Restore original colors (optional - or keep blue to show it died while frozen)
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j].color = originalColors[i][j];
                    mats[j].SetFloat("_Metallic", 0f);
                }
            }
        }
        
        // Activate ragdoll
        if (ragdollOnOff != null)
        {
            ragdollOnOff.RagdollModeOn();
        }
        
        Destroy(gameObject, 5f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
    
    public void HitByExplosion()
    {
        // Don't ragdoll from explosion while frozen
        if (isDead || isRagdolled || isFrozen) return;
        
        isRagdolled = true;
        
        RagdollOnOff ragdoll = GetComponent<RagdollOnOff>();
        if (ragdoll != null)
        {
            ragdoll.RagdollModeOn();
        }
        
        StartCoroutine(WaitForRagdollToSettle());
    }

    private void AlignPositionToHips()
    {
        if (hipsBone == null) return;
    
        Vector3 originalHipsPosition = hipsBone.position;
        transform.position = hipsBone.position;

        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
        }

        hipsBone.position = originalHipsPosition;
    }
    
    public void Freeze(float duration)
    {
        if (isDead || isFrozen) return;
        
        isFrozen = true;
        
        // Disable animator
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Change to blue frozen color
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.color = new Color(0.5f, 0.7f, 1f, 1f);
                mat.SetFloat("_Metallic", 0.8f);
            }
        }
        
        Invoke(nameof(Unfreeze), duration);
    }

    void Unfreeze()
    {
        if (isDead) return;
    
        isFrozen = false;
        
        // If ragdolled while frozen, recover now
        if (isRagdolled)
        {
            StartCoroutine(WaitForRagdollToSettle());
        }
        
        // Re-enable animator
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("Armature|Idle");
        }
        
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].color = originalColors[i][j];
                mats[j].SetFloat("_Metallic", 0f);
            }
        }
    }
    public bool IsFrozen()
    {
        return isFrozen;
    }
}