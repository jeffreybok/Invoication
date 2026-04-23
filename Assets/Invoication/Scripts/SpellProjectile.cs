using UnityEngine;
using PurrNet;

public class SpellProjectile : NetworkBehaviour
{
    public enum SpellType
    {
        Fireball,
        BlazingImpact,
        DragonsBreath,
        Iceball,
        FireWall,
        IceWall,
        LightningStrike,
    }

    [Header("Spell Settings")]
    public SpellType spellType;
    public float directDamage = 30f;
    public float splashRadius = 0f;
    public float splashDamageMult = 0.5f;

    [Header("Burn (Fire spells)")]
    public float burnDamagePerTick = 5f;
    public float burnDuration = 3f;

    [Header("Freeze (Ice spells)")]
    public float freezeDuration = 3f;

    [Header("FireWall")]
    public GameObject fireWallPrefab;
    public float fireWallTravelDistance = 5f;
    public float fireWallLifetime = 6f;
    public float fireWallBurnDamagePerTick = 8f;
    public float fireWallBurnDuration = 4f;
    public float fireWallTickRate = 0.5f;

    [Header("IceWall")]
    public GameObject iceWallPrefab;
    public float iceWallTravelDistance = 5f;
    public float iceWallLifetime = 6f;
    public float iceWallFreezeDuration = 3f;
    public float iceWallTickRate = 0.5f;

    [Header("Lightning Strike")]
    public float lightningChainRadius = 6f;
    public GameObject lightningImpactVFX;
    public GameObject lightningChainVFX;

    [Header("VFX")]
    public GameObject impactVFX;
    public float vfxDuration = 2f;

    private bool _fireWallDeployed = false;
    private bool _iceWallDeployed = false;
    private Vector3 _spawnPosition;
    private Vector3 _travelDirection;

    private GameObject shooter;
    private bool hasHit = false;

    public void SetOwner(GameObject newShooter)
    {
        shooter = newShooter;

        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null) return;

        Collider[] shooterColliders = newShooter.GetComponentsInChildren<Collider>();
        foreach (Collider col in shooterColliders)
            Physics.IgnoreCollision(myCollider, col);
    }

    public void SetTravelDirection(Vector3 dir)
    {
        _travelDirection = dir.normalized;
        Debug.Log("Travel direction set: " + _travelDirection);
    }

    void Start()
    {
        if (spellType == SpellType.FireWall || spellType == SpellType.IceWall)
            _spawnPosition = transform.position;
    }

    void Update()
    {
        if (!isServer) return;

        if (spellType == SpellType.FireWall && !_fireWallDeployed)
        {
            float dist = Vector3.Distance(_spawnPosition, transform.position);
            if (dist >= fireWallTravelDistance)
                DeployFireWallInternal(transform.position);
        }

        if (spellType == SpellType.IceWall && !_iceWallDeployed)
        {
            float dist = Vector3.Distance(_spawnPosition, transform.position);
            if (dist >= iceWallTravelDistance)
                DeployIceWallInternal(transform.position);
        }

        if (spellType == SpellType.LightningStrike && !hasHit)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 dir = _travelDirection != Vector3.zero ? _travelDirection : transform.forward;
            float checkDistance = rb != null ? rb.linearVelocity.magnitude * Time.deltaTime * 2f : 3f;

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, checkDistance))
            {
                if (shooter != null && hit.transform.root.gameObject == shooter) return;
                hasHit = true;
                HandleLightningStrike(null, hit.point);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        if (shooter != null && collision.transform.root.gameObject == shooter)
            return;

#if UNITY_EDITOR
        HandleHitInternal(collision);
#else
        if (isServer)
            HandleHitInternal(collision);
        else
            RequestHit_ServerRPC();
#endif
    }

    [ServerRpc]
    void RequestHit_ServerRPC()
    {
        if (hasHit) return;
        HandleHitInternal(null);
    }

    void HandleHitInternal(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Vector3 point = transform.position;
        if (collision != null && collision.contacts.Length > 0)
            point = collision.contacts[0].point;

        if (spellType == SpellType.FireWall)
        {
            DeployFireWallInternal(point);
            return;
        }

        if (spellType == SpellType.IceWall)
        {
            DeployIceWallInternal(point);
            return;
        }

        if (spellType == SpellType.LightningStrike)
        {
            HandleLightningStrike(collision, point);
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(point, splashRadius > 0f ? splashRadius : 0.1f);

        foreach (Collider hit in hitColliders)
        {
            // Never hit the shooter
            if (shooter != null && hit.transform.root.gameObject == shooter) continue;

            // Hit enemies
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
                ApplyEffect(enemy);

            // Friendly fire: hit other players (Iceball also freezes + colors them)
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null && spellType != SpellType.LightningStrike)
                ApplyEffectToPlayer(playerHealth);

            // Physics force
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
                ApplyForce_ObserversRPC(rb.gameObject, point);
        }

#if UNITY_EDITOR
        SpawnVFX(point);
#else
        PlayImpactVFX_ObserversRPC(point);
#endif
        Destroy(gameObject);
    }

    // ─── Lightning Strike ─────────────────────────────────────────────────────
    // Lightning does NOT apply friendly fire

    void HandleLightningStrike(Collision collision, Vector3 point)
    {
        Enemy primaryEnemy = null;

        if (collision != null)
            primaryEnemy = collision.collider.GetComponentInParent<Enemy>();

        if (primaryEnemy == null)
        {
            Vector3 dir = _travelDirection != Vector3.zero ? _travelDirection : transform.forward;
            if (Physics.Raycast(transform.position, dir, out RaycastHit rayHit, 3f))
                primaryEnemy = rayHit.collider.GetComponentInParent<Enemy>();
        }

        if (primaryEnemy != null)
            primaryEnemy.TakeDamage(directDamage, shooter);

#if UNITY_EDITOR
        SpawnLightningVFX(point, true);
#else
        PlayLightningImpactVFX_ObserversRPC(point);
#endif

        Collider[] nearby = Physics.OverlapSphere(point, lightningChainRadius);
        foreach (Collider hit in nearby)
        {
            if (shooter != null && hit.transform.root.gameObject == shooter) continue;

            Enemy chainEnemy = hit.GetComponentInParent<Enemy>();
            if (chainEnemy == null || chainEnemy == primaryEnemy) continue;

            chainEnemy.TakeDamage(directDamage * splashDamageMult, shooter);

#if UNITY_EDITOR
            SpawnLightningVFX(hit.transform.position, false);
#else
            PlayLightningChainVFX_ObserversRPC(hit.transform.position);
#endif
        }

        Destroy(gameObject);
    }

    // ─── Effect Dispatch ──────────────────────────────────────────────────────

    void ApplyEffect(Enemy enemy)
    {
        switch (spellType)
        {
            case SpellType.Fireball:
                enemy.TakeDamage(directDamage, shooter);
                break;

            case SpellType.BlazingImpact:
                enemy.TakeDamage(directDamage, shooter);
                enemy.ApplyBurn(burnDamagePerTick, burnDuration, shooter);
                break;

            case SpellType.DragonsBreath:
                enemy.TakeDamage(directDamage, shooter);
                enemy.ApplyBurn(burnDamagePerTick, burnDuration * 0.5f, shooter);
                break;

            case SpellType.Iceball:
                enemy.TakeDamage(directDamage, shooter);
                enemy.Freeze(freezeDuration, shooter);
                break;
        }
    }

    // Applies spell effects to a player (friendly fire)
    // Iceball also freezes the player and turns them bright blue
    void ApplyEffectToPlayer(PlayerHealth playerHealth)
    {
        NetworkIdentity netId = playerHealth.GetComponent<NetworkIdentity>();
        if (netId == null) return;

        DamagePlayer_ServerRPC(netId, directDamage);

        if (spellType == SpellType.Iceball)
            FreezePlayer_ServerRPC(netId, freezeDuration);
    }

    // ─── Player Damage / Freeze RPCs ──────────────────────────────────────────

    [ServerRpc(requireOwnership: false)]
    void DamagePlayer_ServerRPC(NetworkIdentity targetIdentity, float damage)
    {
        if (!isServer) return;
        if (targetIdentity == null) return;

        PlayerHealth playerHealth = targetIdentity.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(damage);
    }

    [ServerRpc(requireOwnership: false)]
    void FreezePlayer_ServerRPC(NetworkIdentity targetIdentity, float duration)
    {
        if (!isServer) return;
        if (targetIdentity == null) return;

        PlayerHealth playerHealth = targetIdentity.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Freeze(duration);
    }

    // ─── FireWall / IceWall ───────────────────────────────────────────────────

    void DeployFireWallInternal(Vector3 position)
    {
        if (_fireWallDeployed) return;
        _fireWallDeployed = true;

        position.y += 4f;
        SpawnFireWall_ObserversRPC(position, transform.rotation);
        Destroy(gameObject);
    }

    void DeployIceWallInternal(Vector3 position)
    {
        if (_iceWallDeployed) return;
        _iceWallDeployed = true;

        SpawnIceWall_ObserversRPC(position, transform.rotation);
        Destroy(gameObject);
    }

    [ObserversRpc]
    void SpawnFireWall_ObserversRPC(Vector3 pos, Quaternion rot)
    {
        if (fireWallPrefab == null) return;

        GameObject wall = Instantiate(fireWallPrefab, pos, rot);
        FireWall fw = wall.GetComponent<FireWall>();
        if (fw != null)
        {
            fw.lifetime = fireWallLifetime;
            fw.burnDamagePerTick = fireWallBurnDamagePerTick;
            fw.burnDuration = fireWallBurnDuration;
            fw.tickRate = fireWallTickRate;
        }

        Destroy(wall, fireWallLifetime);
    }

    [ObserversRpc]
    void SpawnIceWall_ObserversRPC(Vector3 pos, Quaternion rot)
    {
        if (iceWallPrefab == null) return;

        GameObject wall = Instantiate(iceWallPrefab, pos, rot);
        IceWall iw = wall.GetComponent<IceWall>();
        if (iw != null)
        {
            iw.lifetime = iceWallLifetime;
            iw.freezeDuration = iceWallFreezeDuration;
            iw.tickRate = iceWallTickRate;
        }

        Destroy(wall, iceWallLifetime);
    }

    // ─── RPCs ─────────────────────────────────────────────────────────────────

    [ObserversRpc]
    void ApplyForce_ObserversRPC(GameObject target, Vector3 origin)
    {
        if (target == null) return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
            rb.AddExplosionForce(200f, origin, splashRadius, 0.5f, ForceMode.Impulse);
    }

    [ObserversRpc]
    void PlayImpactVFX_ObserversRPC(Vector3 pos)
    {
        SpawnVFX(pos);
    }

    [ObserversRpc]
    void PlayLightningImpactVFX_ObserversRPC(Vector3 pos)
    {
        SpawnLightningVFX(pos, true);
    }

    [ObserversRpc]
    void PlayLightningChainVFX_ObserversRPC(Vector3 pos)
    {
        SpawnLightningVFX(pos, false);
    }

    void SpawnVFX(Vector3 pos)
    {
        if (impactVFX == null) return;
        GameObject vfx = Instantiate(impactVFX, pos, Quaternion.identity);
        Destroy(vfx, vfxDuration);
    }

    void SpawnLightningVFX(Vector3 pos, bool isPrimary)
    {
        GameObject prefab = isPrimary ? lightningImpactVFX : lightningChainVFX;
        if (prefab == null) return;
        GameObject vfx = Instantiate(prefab, pos, Quaternion.identity);
        Destroy(vfx, vfxDuration);
    }

    void OnDrawGizmosSelected()
    {
        if (spellType == SpellType.LightningStrike)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, lightningChainRadius);
        }
        else
        {
            Gizmos.color = spellType == SpellType.Iceball ? Color.cyan : Color.red;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }
}