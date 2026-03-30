using UnityEngine;
using UnityEngine.AI;
using PurrNet;
using System.Collections;

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

    [Header("Freeze Settings")]
    public Color frozenColor = Color.cyan;

    [Header("Burn Settings")]
    public Color burningColor = new Color(1f, 0.4f, 0f);

    private float lastAttackTime;
    private PlayerHealth playerHealth;

    private bool isFrozen = false;
    private bool isDead = false;
    private bool isRagdolled = false;
    private bool isBurning = false;
    private bool playerInSight = false;
    private bool isChasing = false;

    private float timeSinceLastSeen = 0f;

    private Renderer[] renderers;
    private Color[][] originalColors;

    private Animator animator;
    private RagdollOnOff ragdollOnOff;
    private Transform hipsBone;
    private NavMeshAgent navAgent;

    private Coroutine burnRoutine;
    private Coroutine ragdollRecoverRoutine;
    private Coroutine freezeRoutine;

    private GameObject lastAttacker;

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

        CacheRenderersAndColors();

        if (animator != null)
            animator.Play("Armature|Idle");

        if (isServer)
            SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
    }

    void Update()
    {
        if (!isServer) return;
        if (isDead) return;
        if (isFrozen) return;
        if (isRagdolled) return;

        FindClosestPlayer();
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

        UpdateAnimatorMovement();
    }

    void CacheRenderersAndColors()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = new Color[renderers[i].materials.Length];

            for (int j = 0; j < renderers[i].materials.Length; j++)
                originalColors[i][j] = renderers[i].materials[j].color;
        }
    }

    void UpdateAnimatorMovement()
    {
        if (animator == null) return;
        if (navAgent == null) return;

        bool moving = navAgent.enabled && navAgent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", moving);
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = Mathf.Infinity;
        Transform closest = null;
        PlayerHealth closestHealth = null;

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
                closestHealth = ph;
            }
        }

        player = closest;
        playerHealth = closestHealth;
    }

    bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector3 start = transform.position + Vector3.up * 1.5f;
        Vector3 end = player.position + Vector3.up;
        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        if (Physics.Raycast(start, dir, out RaycastHit hit, distance, obstacleMask))
            return false;

        return true;
    }

    void ChasePlayer()
    {
        if (navAgent == null) return;
        if (!navAgent.enabled) return;
        if (player == null) return;

        navAgent.SetDestination(player.position);
    }

    void StopChasing()
    {
        if (navAgent == null) return;
        if (!navAgent.enabled) return;

        navAgent.ResetPath();
    }

    void TryAttackPlayer(float distance)
    {
        if (playerHealth == null) return;
        if (playerHealth.isDead) return;

        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            playerHealth.TakeDamage(attackDamage);
        }
    }

    // =========================
    // PUBLIC STATE HELPERS
    // =========================

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsBurning()
    {
        return isBurning;
    }

    public bool IsRagdolled()
    {
        return isRagdolled;
    }

    public GameObject GetLastAttacker()
    {
        return lastAttacker;
    }

    // =========================
    // PICKUP DAMAGE
    // =========================

    public void OnHitByPickup()
    {
        OnHitByPickup(null);
    }

    public void OnHitByPickup(GameObject attacker)
    {
        if (isServer)
        {
            HandleHitByPickup(attacker);
        }
        else
        {
            OnHitByPickup_ServerRPC(attacker);
        }
    }

    [ServerRpc]
    void OnHitByPickup_ServerRPC(GameObject attacker)
    {
        HandleHitByPickup(attacker);
    }

    void HandleHitByPickup(GameObject attacker)
    {
        if (!isServer) return;
        if (isDead) return;

        ApplyDamageInternal(30f, attacker);

        if (isDead) return;

        if (!isFrozen)
        {
            EnterRagdollInternal();

            if (ragdollRecoverRoutine != null)
                StopCoroutine(ragdollRecoverRoutine);

            ragdollRecoverRoutine = StartCoroutine(WaitForRagdollToSettle());
        }
        else
        {
            isRagdolled = true;
            SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        }
    }

    // =========================
    // EXPLOSION DAMAGE
    // =========================

    public void HitByExplosion()
    {
        HitByExplosion(null);
    }

    public void HitByExplosion(GameObject attacker)
    {
        if (isServer)
        {
            HandleHitByExplosion(attacker);
        }
        else
        {
            HitByExplosion_ServerRPC(attacker);
        }
    }

    [ServerRpc]
    void HitByExplosion_ServerRPC(GameObject attacker)
    {
        HandleHitByExplosion(attacker);
    }

    void HandleHitByExplosion(GameObject attacker)
    {
        if (!isServer) return;
        if (isDead) return;
        if (isRagdolled) return;
        if (isFrozen) return;

        RememberAttacker(attacker);

        EnterRagdollInternal();

        if (ragdollRecoverRoutine != null)
            StopCoroutine(ragdollRecoverRoutine);

        ragdollRecoverRoutine = StartCoroutine(WaitForRagdollToSettle());
    }

    IEnumerator WaitForRagdollToSettle()
    {
        yield return new WaitForSeconds(1f);

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        bool moving = true;

        while (moving)
        {
            if (isDead)
                yield break;

            if (isFrozen)
                yield break;

            moving = false;

            foreach (Rigidbody rb in rbs)
            {
                if (rb == null) continue;

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

        ragdollRecoverRoutine = null;
    }

    void RecoverFromRagdoll()
    {
        if (!isServer) return;
        if (isDead) return;

        isRagdolled = false;

        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOff();

        if (navAgent != null && !navAgent.enabled)
            navAgent.enabled = true;

        if (animator != null)
            animator.Play("Armature|Idle");

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
    }

    // =========================
    // FREEZE
    // =========================

    public void Freeze(float duration)
    {
        Freeze(duration, null);
    }

    public void Freeze(float duration, GameObject attacker)
    {
        if (isServer)
        {
            FreezeInternal(duration, attacker);
        }
        else
        {
            Freeze_ServerRPC(duration, attacker);
        }
    }

    public void Freeze_Server(float duration, GameObject attacker)
    {
        if (!isServer) return;
        FreezeInternal(duration, attacker);
    }

    [ServerRpc]
    void Freeze_ServerRPC(float duration, GameObject attacker)
    {
        FreezeInternal(duration, attacker);
    }

    void FreezeInternal(float duration, GameObject attacker)
    {
        if (!isServer) return;
        if (isDead) return;
        if (duration <= 0f) return;

        RememberAttacker(attacker);

        isFrozen = true;

        if (ragdollRecoverRoutine != null)
        {
            StopCoroutine(ragdollRecoverRoutine);
            ragdollRecoverRoutine = null;
        }

        if (navAgent != null)
        {
            if (navAgent.enabled)
                navAgent.ResetPath();

            navAgent.enabled = false;
        }

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);

        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeDurationCoroutine(duration));
    }

    IEnumerator FreezeDurationCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        Unfreeze();
        freezeRoutine = null;
    }

    void Unfreeze()
    {
        if (!isServer) return;
        if (isDead) return;

        isFrozen = false;

        if (navAgent != null && !navAgent.enabled && !isRagdolled)
            navAgent.enabled = true;

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);
    }

    // =========================
    // BURN
    // =========================

    public void ApplyBurn(float damagePerTick, float duration)
    {
        ApplyBurn(damagePerTick, duration, 0.5f, null);
    }

    public void ApplyBurn(float damagePerTick, float duration, GameObject attacker)
    {
        ApplyBurn(damagePerTick, duration, 0.5f, attacker);
    }

    public void ApplyBurn(float damagePerTick, float duration, float tickInterval)
    {
        ApplyBurn(damagePerTick, duration, tickInterval, null);
    }

    public void ApplyBurn(float damagePerTick, float duration, float tickInterval, GameObject attacker)
    {
        if (isServer)
        {
            StartBurnInternal(damagePerTick, duration, tickInterval, attacker);
        }
        else
        {
            ApplyBurn_ServerRPC(damagePerTick, duration, tickInterval, attacker);
        }
    }

    public void ApplyBurn_Server(float damagePerTick, float duration, GameObject attacker)
    {
        if (!isServer) return;
        StartBurnInternal(damagePerTick, duration, 0.5f, attacker);
    }

    public void ApplyBurn_Server(float damagePerTick, float duration, float tickInterval, GameObject attacker)
    {
        if (!isServer) return;
        StartBurnInternal(damagePerTick, duration, tickInterval, attacker);
    }

    [ServerRpc]
    void ApplyBurn_ServerRPC(float damagePerTick, float duration, float tickInterval, GameObject attacker)
    {
        StartBurnInternal(damagePerTick, duration, tickInterval, attacker);
    }

    void StartBurnInternal(float damagePerTick, float duration, float tickInterval, GameObject attacker)
    {
        if (!isServer) return;
        if (isDead) return;
        if (damagePerTick <= 0f) return;
        if (duration <= 0f) return;
        if (tickInterval <= 0f) tickInterval = 0.5f;

        RememberAttacker(attacker);

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        burnRoutine = StartCoroutine(BurnCoroutine(damagePerTick, duration, tickInterval));
    }

    IEnumerator BurnCoroutine(float damagePerTick, float duration, float tickInterval)
    {
        isBurning = true;

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isDead)
                yield break;

            ApplyDamageInternal(damagePerTick, lastAttacker);

            if (isDead)
                yield break;

            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }

        isBurning = false;

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);

        burnRoutine = null;
    }

    // =========================
    // GENERAL DAMAGE
    // =========================

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(float damage, GameObject attacker)
    {
        if (isServer)
        {
            ApplyDamageInternal(damage, attacker);
        }
        else
        {
            TakeDamage_ServerRPC(damage, attacker);
        }
    }

    public void TakeDamage_Server(float damage, GameObject attacker)
    {
        if (!isServer) return;
        ApplyDamageInternal(damage, attacker);
    }

    [ServerRpc]
    void TakeDamage_ServerRPC(float damage, GameObject attacker)
    {
        ApplyDamageInternal(damage, attacker);
    }

    void ApplyDamageInternal(float damage)
    {
        ApplyDamageInternal(damage, null);
    }

    void ApplyDamageInternal(float damage, GameObject attacker)
    {
        if (!isServer) return;
        if (isDead) return;
        if (damage <= 0f) return;

        RememberAttacker(attacker);

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0f)
        {
            Die(attacker);
            return;
        }

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
    }

    // =========================
    // DEATH / XP
    // =========================

    void Die()
    {
        Die(lastAttacker);
    }

    void Die(GameObject attacker)
    {
        if (!isServer) return;
        if (isDead) return;

        isDead = true;
        isRagdolled = true;
        isFrozen = false;
        isBurning = false;

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        if (ragdollRecoverRoutine != null)
        {
            StopCoroutine(ragdollRecoverRoutine);
            ragdollRecoverRoutine = null;
        }

        if (navAgent != null && navAgent.enabled)
            navAgent.enabled = false;

        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOn();

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);

        AwardXpToAttacker(attacker);

        Destroy(gameObject, 5f);
    }

    void RememberAttacker(GameObject attacker)
    {
        if (attacker == null) return;
        lastAttacker = attacker;
    }

    void AwardXpToAttacker(GameObject attacker)
    {
        GameObject xpOwner = attacker != null ? attacker : lastAttacker;
        if (xpOwner == null) return;

        PlayerXP playerXP = xpOwner.GetComponent<PlayerXP>();

        if (playerXP == null)
            playerXP = xpOwner.GetComponentInParent<PlayerXP>();

        if (playerXP == null)
            playerXP = xpOwner.GetComponentInChildren<PlayerXP>();

        if (playerXP != null)
            playerXP.GainXP(xpReward);
    }

    // =========================
    // SYNC
    // =========================

    [ObserversRpc]
    void SyncState_ObserversRPC(float health, bool dead, bool frozen, bool ragdolled, bool burning)
    {
        currentHealth = health;
        isDead = dead;
        isFrozen = frozen;
        isBurning = burning;

        if (ragdolled && !isRagdolled)
        {
            isRagdolled = true;

            if (navAgent != null && navAgent.enabled)
                navAgent.enabled = false;

            if (ragdollOnOff != null)
                ragdollOnOff.RagdollModeOn();
        }
        else if (!ragdolled && isRagdolled)
        {
            isRagdolled = false;

            if (ragdollOnOff != null)
                ragdollOnOff.RagdollModeOff();

            if (navAgent != null && !isDead && !isFrozen && !navAgent.enabled)
                navAgent.enabled = true;

            if (animator != null && !isDead)
                animator.Play("Armature|Idle");
        }
        else
        {
            isRagdolled = ragdolled;
        }

        if (isDead)
        {
            if (navAgent != null && navAgent.enabled)
                navAgent.enabled = false;

            if (ragdollOnOff != null)
                ragdollOnOff.RagdollModeOn();
        }
    }

    [ObserversRpc]
    void RefreshVisuals_ObserversRPC(bool frozen, bool burning)
    {
        ApplyVisualStateLocal(frozen, burning);
    }

    void ApplyVisualStateLocal(bool frozen, bool burning)
    {
        if (renderers == null || renderers.Length == 0)
            CacheRenderersAndColors();

        RestoreOriginalColors();

        if (burning)
            ApplyColorToAllRenderers(burningColor);

        if (frozen)
            ApplyColorToAllRenderers(frozenColor);
    }

    void ApplyColorToAllRenderers(Color color)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material[] mats = renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
                mats[j].color = color;
        }
    }

    void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            if (i >= originalColors.Length) continue;

            Material[] mats = renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                if (j >= originalColors[i].Length) continue;
                mats[j].color = originalColors[i][j];
            }
        }
    }

    // =========================
    // LEGACY VISUAL WRAPPERS
    // =========================

    [ObserversRpc]
    void SetFrozenVisuals_ObserversRPC(bool frozen)
    {
        ApplyVisualStateLocal(frozen, isBurning);
    }

    [ObserversRpc]
    void SetBurnVisuals_ObserversRPC(bool burning)
    {
        ApplyVisualStateLocal(isFrozen, burning);
    }

    // =========================
    // RAGDOLL
    // =========================

    void EnterRagdollInternal()
    {
        if (!isServer) return;
        if (isDead) return;

        isRagdolled = true;

        if (navAgent != null && navAgent.enabled)
            navAgent.enabled = false;

        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOn();

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
    }

    // =========================
    // OPTIONAL HELPERS FOR OTHER SCRIPTS
    // =========================

    public void DamageFromSpell(float damage, GameObject attacker)
    {
        TakeDamage_Server(damage, attacker);
    }

    public void BurnFromSpell(float damagePerTick, float duration, GameObject attacker)
    {
        ApplyBurn_Server(damagePerTick, duration, attacker);
    }

    public void FreezeFromSpell(float duration, GameObject attacker)
    {
        Freeze_Server(duration, attacker);
    }

    public void DamageFromExplosion(float damage, GameObject attacker, bool forceRagdoll)
    {
        if (!isServer) return;
        if (isDead) return;

        ApplyDamageInternal(damage, attacker);

        if (isDead) return;

        if (forceRagdoll && !isFrozen)
        {
            EnterRagdollInternal();

            if (ragdollRecoverRoutine != null)
                StopCoroutine(ragdollRecoverRoutine);

            ragdollRecoverRoutine = StartCoroutine(WaitForRagdollToSettle());
        }
    }

    public void ForceRememberAttacker(GameObject attacker)
    {
        if (!isServer) return;
        RememberAttacker(attacker);
    }

    public void ClearBurn_Server()
    {
        if (!isServer) return;

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        isBurning = false;
        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);
    }

    public void ClearFreeze_Server()
    {
        if (!isServer) return;

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        isFrozen = false;

        if (navAgent != null && !navAgent.enabled && !isDead && !isRagdolled)
            navAgent.enabled = true;

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);
    }

    public void ResetEnemy_Server()
    {
        if (!isServer) return;

        currentHealth = maxHealth;
        isDead = false;
        isFrozen = false;
        isRagdolled = false;
        isBurning = false;
        lastAttacker = null;

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        if (ragdollRecoverRoutine != null)
        {
            StopCoroutine(ragdollRecoverRoutine);
            ragdollRecoverRoutine = null;
        }

        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOff();

        if (navAgent != null && !navAgent.enabled)
            navAgent.enabled = true;

        if (animator != null)
            animator.Play("Armature|Idle");

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);
    }

    public void KillInstantly_Server(GameObject attacker)
    {
        if (!isServer) return;
        if (isDead) return;

        currentHealth = 0f;
        Die(attacker);
    }

    public void Revive_Server(float healthPercent = 1f)
    {
        if (!isServer) return;

        healthPercent = Mathf.Clamp01(healthPercent);

        currentHealth = maxHealth * healthPercent;
        isDead = false;
        isFrozen = false;
        isRagdolled = false;
        isBurning = false;

        if (ragdollOnOff != null)
            ragdollOnOff.RagdollModeOff();

        if (navAgent != null && !navAgent.enabled)
            navAgent.enabled = true;

        if (animator != null)
            animator.Play("Armature|Idle");

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);
    }

    public float GetHealthPercent()
    {
        if (maxHealth <= 0f) return 0f;
        return currentHealth / maxHealth;
    }

    public void SetHealth_Server(float newHealth, GameObject attacker = null)
    {
        if (!isServer) return;
        if (isDead) return;

        if (attacker != null)
            RememberAttacker(attacker);

        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            Die(attacker);
            return;
        }

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
    }

    public void Heal_Server(float amount)
    {
        if (!isServer) return;
        if (isDead) return;
        if (amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
    }

    public void InterruptAllEffects_Server()
    {
        if (!isServer) return;

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        if (ragdollRecoverRoutine != null)
        {
            StopCoroutine(ragdollRecoverRoutine);
            ragdollRecoverRoutine = null;
        }

        isFrozen = false;
        isBurning = false;

        if (!isDead && !isRagdolled && navAgent != null && !navAgent.enabled)
            navAgent.enabled = true;

        SyncState_ObserversRPC(currentHealth, isDead, isFrozen, isRagdolled, isBurning);
        RefreshVisuals_ObserversRPC(isFrozen, isBurning);
    }

    public void ForceRagdoll_Server(float recoverDelay = 1f)
    {
        if (!isServer) return;
        if (isDead) return;

        EnterRagdollInternal();

        if (ragdollRecoverRoutine != null)
            StopCoroutine(ragdollRecoverRoutine);

        ragdollRecoverRoutine = StartCoroutine(ForceRecoverRagdollCoroutine(recoverDelay));
    }

    IEnumerator ForceRecoverRagdollCoroutine(float recoverDelay)
    {
        yield return new WaitForSeconds(recoverDelay);

        if (!isDead && !isFrozen)
            RecoverFromRagdoll();

        ragdollRecoverRoutine = null;
    }

    public void FacePlayer_Server()
    {
        if (!isServer) return;
        if (player == null) return;
        if (isDead) return;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir.normalized);
    }

    public void StopMovement_Server()
    {
        if (!isServer) return;
        StopChasing();

        if (navAgent != null && navAgent.enabled)
            navAgent.velocity = Vector3.zero;

        UpdateAnimatorMovement();
    }

    public void ResumeMovement_Server()
    {
        if (!isServer) return;
        if (isDead) return;
        if (isFrozen) return;
        if (isRagdolled) return;

        if (navAgent != null && !navAgent.enabled)
            navAgent.enabled = true;
    }

    public void SetMoveSpeed_Server(float newSpeed)
    {
        if (!isServer) return;

        moveSpeed = newSpeed;

        if (navAgent != null)
            navAgent.speed = moveSpeed;
    }

    public void SetAttackDamage_Server(float newDamage)
    {
        if (!isServer) return;
        attackDamage = newDamage;
    }

    public void SetAttackRange_Server(float newRange)
    {
        if (!isServer) return;
        attackRange = newRange;
    }

    public void SetDetectionRadius_Server(float newRadius)
    {
        if (!isServer) return;
        detectionRadius = newRadius;
    }

    public void SetXPReward_Server(int newReward)
    {
        if (!isServer) return;
        xpReward = newReward;
    }

    void OnDisable()
    {
        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        if (ragdollRecoverRoutine != null)
        {
            StopCoroutine(ragdollRecoverRoutine);
            ragdollRecoverRoutine = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, player.position + Vector3.up);
        }
    }
}