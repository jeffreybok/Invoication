using UnityEngine;
using UnityEngine.AI; // Add this for NavMeshAgent

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("References")]
    public Transform player;
    
    [Header("AI Settings")]
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 110f; // How wide their vision is
    public float moveSpeed = 3.5f;
    public LayerMask obstacleMask; // Set this to walls/obstacles in Inspector
    
    [Header("Freeze Settings")]
    private bool isFrozen = false;
    private Renderer[] renderers;
    private Color[][] originalColors;
    
    private Animator animator;
    private RagdollOnOff ragdollOnOff;
    private bool isDead = false;
    private bool isRagdolled = false;
    private Transform hipsBone;
    
    private NavMeshAgent navAgent; // For pathfinding
    private bool playerInSight = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        ragdollOnOff = GetComponent<RagdollOnOff>();
        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        navAgent = GetComponent<NavMeshAgent>();
        
        // Set up NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = 2f; // Stop 2 units away from player
        }
        
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
        
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    
    void Update()
    {
        if (isFrozen || isDead || isRagdolled) return;
        
        // Check if player is in detection range and field of view
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= detectionRadius)
            {
                // Check if player is in field of view
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                
                if (angleToPlayer <= fieldOfViewAngle / 2f)
                {
                    // Check line of sight with raycast
                    if (HasLineOfSight())
                    {
                        playerInSight = true;
                        ChasePlayer();
                    }
                    else
                    {
                        playerInSight = false;
                        StopChasing();
                    }
                }
                else
                {
                    playerInSight = false;
                    StopChasing();
                }
            }
            else
            {
                playerInSight = false;
                StopChasing();
            }
        }
        
        // Update animator based on movement
        if (animator != null && navAgent != null)
        {
            bool isMoving = navAgent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }
    }
    
    bool HasLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Raycast from enemy's eye level to player's center
        Vector3 rayStart = transform.position + Vector3.up * 1.5f; // Adjust height as needed
        Vector3 rayEnd = player.position + Vector3.up * 1f;
        
        // Draw debug ray in scene view
        Debug.DrawRay(rayStart, (rayEnd - rayStart), playerInSight ? Color.green : Color.red);
        
        if (Physics.Raycast(rayStart, (rayEnd - rayStart).normalized, out RaycastHit hit, distanceToPlayer, obstacleMask))
        {
            // Something is blocking the view
            return false;
        }
        
        return true;
    }
    
    void ChasePlayer()
    {
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.SetDestination(player.position);
        }
    }
    
    void StopChasing()
    {
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.ResetPath();
        }
    }

    public void OnHitByPickup()
    {
        TakeDamage(30f);
        
        if (!isFrozen && !isDead)
        {
            isRagdolled = true;
            
            // Disable NavMeshAgent during ragdoll
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
            
            StartCoroutine(WaitForRagdollToSettle());
        }
        else if (isFrozen && !isDead)
        {
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
        
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        
        // Re-enable NavMeshAgent
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }
        
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
        
        // Disable NavMeshAgent
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        if (isFrozen)
        {
            CancelInvoke(nameof(Unfreeze));
            isFrozen = false;
            
            if (animator != null)
            {
                animator.enabled = true;
            }
            
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
        
        if (ragdollOnOff != null)
        {
            ragdollOnOff.RagdollModeOn();
        }
        
        Destroy(gameObject, 5f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Field of view
        Gizmos.color = Color.blue;
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2f, Vector3.up) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2f, Vector3.up) * transform.forward * detectionRadius;
        
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
    
    public void HitByExplosion()
    {
        if (isDead || isRagdolled || isFrozen) return;
        
        isRagdolled = true;
        
        // Disable NavMeshAgent
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
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
        
        // Stop NavMeshAgent
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }
        
        if (animator != null)
        {
            animator.enabled = false;
        }
        
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
        
        // Re-enable NavMeshAgent
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }
        
        if (isRagdolled)
        {
            StartCoroutine(WaitForRagdollToSettle());
        }
        
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("Armature|Idle");
        }
        
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