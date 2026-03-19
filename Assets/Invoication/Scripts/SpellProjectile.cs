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

    void Start()
    {
        if (spellType == SpellType.FireWall || spellType == SpellType.IceWall)
            _spawnPosition = transform.position;
    }

    void Update()
    {
        if (spellType == SpellType.FireWall && !_fireWallDeployed)
        {
            float distanceTraveled = Vector3.Distance(_spawnPosition, transform.position);
            if (distanceTraveled >= fireWallTravelDistance)
                DeployFireWall(transform.position);
        }
        if (spellType == SpellType.IceWall && !_iceWallDeployed)
        {
            float distanceTraveled = Vector3.Distance(_spawnPosition, transform.position);
            if (distanceTraveled >= iceWallTravelDistance)
                DeployIceWall(transform.position);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
<<<<<<< HEAD
        if (spellType == SpellType.FireWall)
        {
            DeployFireWall(collision.contacts[0].point);
            return;
        }
        if (spellType == SpellType.IceWall)
        {
            DeployIceWall(collision.contacts[0].point);
            return;
        }
=======
        if (!isServer) return;
>>>>>>> main

        Vector3 point = collision.contacts[0].point;

        Enemy directEnemy = collision.gameObject.GetComponent<Enemy>();
        if (directEnemy != null)
            ApplyEffect(directEnemy);

        Collider[] hitColliders = Physics.OverlapSphere(point, splashRadius > 0f ? splashRadius : 0.1f);

        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy != null && enemy != directEnemy)
                ApplySplashEffect(enemy);

            if (spellType == SpellType.Fireball || spellType == SpellType.BlazingImpact)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                    rb.AddExplosionForce(200f, point, splashRadius, 0.5f, ForceMode.Impulse);
            }
        }

        if (directEnemy != null && (spellType == SpellType.Fireball || spellType == SpellType.BlazingImpact))
            directEnemy.HitByExplosion();

        if (impactVFX != null)
        {
            ParticleSystem ps = impactVFX.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : vfxDuration;

            GameObject vfx = Instantiate(impactVFX, point, Quaternion.identity);
            Destroy(vfx, duration);
        }

        Destroy(gameObject);
    }

    void DeployFireWall(Vector3 position)
    {
        if (_fireWallDeployed) return;
        _fireWallDeployed = true;

        position.y += 4f;

        if (fireWallPrefab != null)
        {
            GameObject wall = Instantiate(fireWallPrefab, position, transform.rotation);
            FireWall fw = wall.GetComponent<FireWall>();
            if (fw != null)
            {
                fw.lifetime          = fireWallLifetime;
                fw.burnDamagePerTick = fireWallBurnDamagePerTick;
                fw.burnDuration      = fireWallBurnDuration;
                fw.tickRate          = fireWallTickRate;
            }
            Destroy(wall, fireWallLifetime);
        }

        Destroy(gameObject);
    }
    
    void DeployIceWall(Vector3 position)
    {
        if (_iceWallDeployed) return;
        _iceWallDeployed = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (iceWallPrefab != null)
        {
            GameObject wall = Instantiate(iceWallPrefab, position, transform.rotation);
            IceWall iw = wall.GetComponent<IceWall>();
            if (iw != null)
            {
                iw.lifetime        = iceWallLifetime;
                iw.freezeDuration  = iceWallFreezeDuration;
                iw.tickRate        = iceWallTickRate;
            }
            Destroy(wall, iceWallLifetime);
        }

        Destroy(gameObject);
    }

    void ApplyEffect(Enemy enemy)
    {
        switch (spellType)
        {
            case SpellType.Fireball:
                float fireballDmg = PlayerStats.Instance != null
                    ? PlayerStats.Instance.GetFireballDamage()
                    : directDamage;
                enemy.TakeDamage(fireballDmg);
                break;

            case SpellType.BlazingImpact:
                float blazingDmg = PlayerStats.Instance != null
                    ? PlayerStats.Instance.GetBlazingDamage()
                    : directDamage;
                float burnTick = PlayerStats.Instance != null
                    ? PlayerStats.Instance.blazingBurnDamagePerTick
                    : burnDamagePerTick;
                float burnDur = PlayerStats.Instance != null
                    ? PlayerStats.Instance.blazingBurnDuration
                    : burnDuration;
                
                enemy.TakeDamage(blazingDmg);
                enemy.ApplyBurn(burnTick, burnDur);
                break;

            case SpellType.DragonsBreath:
                enemy.TakeDamage(directDamage);
                enemy.ApplyBurn(burnDamagePerTick, burnDuration * 0.5f);
                break;

            case SpellType.Iceball:
                float iceDmg = PlayerStats.Instance != null
                    ? PlayerStats.Instance.GetIceSpikeDamage()
                    : directDamage;
                float freezeDur = PlayerStats.Instance != null
                    ? PlayerStats.Instance.iceSpikeSlowDuration
                    : freezeDuration;
                
                enemy.TakeDamage(iceDmg);
                enemy.Freeze(freezeDur);
                break;
        }
    }

    void ApplySplashEffect(Enemy enemy)
    {
        switch (spellType)
        {
            case SpellType.Fireball:
                float fireballSplashDmg = PlayerStats.Instance != null
                    ? PlayerStats.Instance.GetFireballDamage()
                    : directDamage;
                enemy.TakeDamage(fireballSplashDmg * splashDamageMult);
                break;

            case SpellType.BlazingImpact:
                float blazingSplashDmg = PlayerStats.Instance != null
                    ? PlayerStats.Instance.GetBlazingDamage()
                    : directDamage;
                float splashBurnTick = PlayerStats.Instance != null
                    ? PlayerStats.Instance.blazingBurnDamagePerTick
                    : burnDamagePerTick;
                float splashBurnDur = PlayerStats.Instance != null
                    ? PlayerStats.Instance.blazingBurnDuration
                    : burnDuration;

                enemy.TakeDamage(blazingSplashDmg * splashDamageMult);
                enemy.ApplyBurn(splashBurnTick, splashBurnDur);
                break;

            case SpellType.Iceball:
                float splashFreezeDur = PlayerStats.Instance != null
                    ? PlayerStats.Instance.iceSpikeSlowDuration
                    : freezeDuration;
                enemy.Freeze(splashFreezeDur);
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = spellType == SpellType.Iceball ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}