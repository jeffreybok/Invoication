using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("References")]
    public Transform player;
    
    [Header("AI Settings")]
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 110f;
    public float moveSpeed = 3.5f;
    public LayerMask obstacleMask;
    public float chaseMemoryTime = 3f;

    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 2f;

    private float lastAttackTime;
    private PlayerHealth playerHealth;

    [Header("Freeze Settings")]
    private bool isFrozen = false;
    private Renderer[] renderers;
    private Color[][] originalColors;

    private Animator animator;
    private RagdollOnOff ragdollOnOff;
    private bool isDead = false;
    private bool isRagdolled = false;
    private Transform hipsBone;
    
    private NavMeshAgent navAgent;
    private bool playerInSight = false;
    private bool isChasing = false;
    private float timeSinceLastSeen = 0f;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        ragdollOnOff = GetComponent<RagdollOnOff>();
        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        navAgent = GetComponent<NavMeshAgent>();
        
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = 2f;
        }
        
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
            animator.Play("Armature|Idle");
        
        // Always find playerHealth regardless of how player was assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // This now always runs
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }
    
    void Update()
    {
        if (isFrozen || isDead || isRagdolled) return;
    
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            bool canSeePlayer = false;
            
            if (distanceToPlayer <= detectionRadius)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                
                if (angleToPlayer <= fieldOfViewAngle / 2f && HasLineOfSight())
                {
                    canSeePlayer = true;
                }
            }
            
            if (canSeePlayer)
            {
                playerInSight = true;
                isChasing = true;
                timeSinceLastSeen = 0f;
                ChasePlayer();
                TryAttackPlayer(distanceToPlayer);
            }
            else
            {
                playerInSight = false;
                
                if (isChasing)
                {
                    timeSinceLastSeen += Time.deltaTime;
                    
                    if (timeSinceLastSeen < chaseMemoryTime)
                    {
                        ChasePlayer();
                    }
                    else
                    {
                        isChasing = false;
                        StopChasing();
                    }
                }
            }
        }
        
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
        
        Vector3 rayStart = transform.position + Vector3.up * 1.5f;
        Vector3 rayEnd = player.position + Vector3.up * 1f;
        
        Debug.DrawRay(rayStart, (rayEnd - rayStart), playerInSight ? Color.green : Color.red);
        
        if (Physics.Raycast(rayStart, (rayEnd - rayStart).normalized, out RaycastHit hit, distanceToPlayer, obstacleMask))
        {
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

    void TryAttackPlayer(float distanceToPlayer)
    {
        if (playerHealth == null || playerHealth.isDead)
            return;

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("Goblin attacked player for " + attackDamage + " damage!");
        }
    }

    public void OnHitByPickup()
    {
        TakeDamage(30f);
        
        if (!isFrozen && !isDead)
        {
            isRagdolled = true;
            
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
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

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
        }

        hipsBone.position = originalHipsPosition;
    }
    
    public void Freeze(float duration)
    {
        if (isDead || isFrozen) return;
        
        isFrozen = true;
        
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