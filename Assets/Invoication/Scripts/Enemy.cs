using UnityEngine;
using UnityEngine.AI;
using PurrNet;

public class Enemy : NetworkBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int xpReward = 50;

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
    private bool isChasing = false;
    private float timeSinceLastSeen = 0f;

    private bool isBurning = false;

    private GameObject lastDamageDealer;

    // OPTIMIZATION: don't search every frame
    private float playerSearchTimer = 0f;
    private float playerSearchInterval = 0.3f;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        ragdollOnOff = GetComponent<RagdollOnOff>();
        navAgent = GetComponent<NavMeshAgent>();

        if (animator != null)
            hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);

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
                originalColors[i][j] = renderers[i].materials[j].color;
        }

        if (animator != null)
            animator.Play("Armature|Idle");
    }

    void Update()
    {
        if (!isServer) return;
        if (isFrozen || isDead || isRagdolled) return;

        // Only search periodically
        playerSearchTimer += Time.deltaTime;
        if (playerSearchTimer >= playerSearchInterval)
        {
            playerSearchTimer = 0f;
            FindClosestPlayer();
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = false;

        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= fieldOfViewAngle / 2f && HasLineOfSight())
                canSeePlayer = true;
        }

        if (canSeePlayer)
        {
            isChasing = true;
            timeSinceLastSeen = 0f;

            ChasePlayer();
            TryAttackPlayer(distanceToPlayer);
        }
        else
        {
            if (isChasing)
            {
                timeSinceLastSeen += Time.deltaTime;

                if (timeSinceLastSeen < chaseMemoryTime)
                    ChasePlayer();
                else
                {
                    isChasing = false;
                    StopChasing();
                }
            }
        }

        if (animator != null && navAgent != null)
        {
            bool moving = navAgent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", moving);
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            if (p == null) continue;

            PlayerHealth ph = p.GetComponent<PlayerHealth>();
            if (ph != null && ph.isDead) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p.transform;
            }
        }

        if (closest != null)
        {
            player = closest;
            playerHealth = closest.GetComponent<PlayerHealth>();
        }
    }

    bool HasLineOfSight()
    {
        Vector3 start = transform.position + Vector3.up * 1.5f;
        Vector3 end = player.position + Vector3.up;

        float distance = Vector3.Distance(start, end);

        if (Physics.Raycast(start, (end - start).normalized, out RaycastHit hit, distance, obstacleMask))
            return false;

        return true;
    }

    void ChasePlayer()
    {
        if (navAgent != null && navAgent.enabled)
            navAgent.SetDestination(player.position);
    }

    void StopChasing()
    {
        if (navAgent != null && navAgent.enabled)
            navAgent.ResetPath();
    }

    void TryAttackPlayer(float distance)
    {
        if (playerHealth == null || playerHealth.isDead) return;

        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void OnHitByPickup()
    {
        TakeDamage(30f);

        if (!isFrozen && !isDead)
        {
            isRagdolled = true;

            if (navAgent != null)
                navAgent.enabled = false;

            if (ragdollOnOff != null)
                ragdollOnOff.RagdollModeOn();

            StartCoroutine(WaitForRagdollToSettle());
        }
        else if (isFrozen && !isDead)
        {
            isRagdolled = true;
        }
    }

    public void HitByExplosion()
    {
        if (isDead || isRagdolled || isFrozen) return;

        isRagdolled = true;

        if (navAgent != null)
            navAgent.enabled = false;

        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOn();

        StartCoroutine(WaitForRagdollToSettle());
    }

    System.Collections.IEnumerator WaitForRagdollToSettle()
    {
        yield return new WaitForSeconds(1f);

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        bool moving = true;

        while (moving)
        {
            moving = false;

            foreach (Rigidbody rb in rbs)
            {
                if (rb.linearVelocity.magnitude > 0.1f || rb.angularVelocity.magnitude > 0.1f)
                {
                    moving = true;
                    break;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (!isDead && !isFrozen)
            RecoverFromRagdoll();
    }

    void RecoverFromRagdoll()
    {
        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOff();

        isRagdolled = false;

        if (navAgent != null)
            navAgent.enabled = true;

        if (animator != null)
            animator.Play("Armature|Idle");
    }

    public void Freeze(float duration)
    {
        if (isDead || isFrozen) return;

        isFrozen = true;

        foreach (Renderer r in renderers)
            foreach (Material m in r.materials)
                m.color = Color.cyan;

        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        Invoke(nameof(Unfreeze), duration);
    }

    void Unfreeze()
    {
        if (isDead) return;

        isFrozen = false;

        for (int i = 0; i < renderers.Length; i++)
            for (int j = 0; j < renderers[i].materials.Length; j++)
                renderers[i].materials[j].color = originalColors[i][j];

        if (navAgent != null)
            navAgent.enabled = true;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public void ApplyBurn(float damagePerTick, float duration, float tickInterval = 0.5f)
    {
        if (isDead) return;

        if (isBurning)
            StopCoroutine(nameof(BurnCoroutine));

        StartCoroutine(BurnCoroutine(damagePerTick, duration, tickInterval));
    }

    System.Collections.IEnumerator BurnCoroutine(float damagePerTick, float duration, float tickInterval)
    {
        isBurning = true;

        foreach (Renderer r in renderers)
            foreach (Material m in r.materials)
                m.color = new Color(1f, 0.4f, 0f);

        float elapsed = 0f;

        while (elapsed < duration && !isDead)
        {
            TakeDamage(damagePerTick);
            elapsed += tickInterval;

            yield return new WaitForSeconds(tickInterval);
        }

        for (int i = 0; i < renderers.Length; i++)
            for (int j = 0; j < renderers[i].materials.Length; j++)
                renderers[i].materials[j].color = originalColors[i][j];

        isBurning = false;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(float damage, GameObject damageDealer)
    {
        if (isDead) return;

        if (damageDealer != null)
            lastDamageDealer = damageDealer;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        isRagdolled = true;

        if (navAgent != null)
            navAgent.enabled = false;

        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOn();

        if (lastDamageDealer != null)
        {
            PlayerXP xp = lastDamageDealer.GetComponent<PlayerXP>();
            if (xp != null)
                xp.GainXP(xpReward);
        }

        Destroy(gameObject, 5f);
    }
}