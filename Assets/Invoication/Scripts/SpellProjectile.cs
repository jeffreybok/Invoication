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

    [Header("VFX")]
    public GameObject impactVFX;
    public float vfxDuration = 2f;

    private bool _fireWallDeployed = false;
    private bool _iceWallDeployed = false;
    private Vector3 _spawnPosition;

    private GameObject shooter;
    private PlayerStats shooterStats;

    private bool hasHit = false;

    public void SetOwner(GameObject newShooter)
    {
        shooter = newShooter;
        shooterStats = newShooter.GetComponent<PlayerStats>();

        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null) return;

        Collider[] shooterColliders = newShooter.GetComponentsInChildren<Collider>();
        foreach (Collider col in shooterColliders)
        {
            Physics.IgnoreCollision(myCollider, col);
        }
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
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        if (shooter != null && collision.transform.root.gameObject == shooter)
            return;

        if (isServer)
        {
            HandleHitInternal(collision);
        }
        else
        {
            RequestHit_ServerRPC();
        }
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

        Collider[] hitColliders = Physics.OverlapSphere(point, splashRadius > 0f ? splashRadius : 0.1f);

        foreach (Collider hit in hitColliders)
        {
            if (shooter != null && hit.transform.root.gameObject == shooter) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                ApplyEffect(enemy);
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                ApplyForce_ObserversRPC(rb.gameObject, point);
            }
        }

        PlayImpactVFX_ObserversRPC(point);
        Destroy(gameObject);
    }

    void ApplyEffect(Enemy enemy)
    {
        switch (spellType)
        {
            case SpellType.Fireball:
                float fireballDmg = shooterStats != null
                    ? shooterStats.GetFireballDamage()
                    : directDamage;

                enemy.TakeDamage(fireballDmg, shooter);
                break;

            case SpellType.BlazingImpact:
                float blazingDmg = shooterStats != null
                    ? shooterStats.GetBlazingDamage()
                    : directDamage;

                enemy.TakeDamage(blazingDmg, shooter);
                enemy.ApplyBurn(burnDamagePerTick, burnDuration, shooter);
                break;

            case SpellType.DragonsBreath:
                enemy.TakeDamage(directDamage, shooter);
                enemy.ApplyBurn(burnDamagePerTick, burnDuration * 0.5f, shooter);
                break;

            case SpellType.Iceball:
                float iceDmg = shooterStats != null
                    ? shooterStats.GetIceSpikeDamage()
                    : directDamage;

                enemy.TakeDamage(iceDmg, shooter);
                enemy.Freeze(freezeDuration, shooter);
                break;
        }
    }

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

    [ObserversRpc]
    void ApplyForce_ObserversRPC(GameObject target, Vector3 origin)
    {
        if (target == null) return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.AddExplosionForce(200f, origin, splashRadius, 0.5f, ForceMode.Impulse);
        }
    }

    [ObserversRpc]
    void PlayImpactVFX_ObserversRPC(Vector3 pos)
    {
        if (impactVFX != null)
        {
            GameObject vfx = Instantiate(impactVFX, pos, Quaternion.identity);
            Destroy(vfx, vfxDuration);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = spellType == SpellType.Iceball ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}