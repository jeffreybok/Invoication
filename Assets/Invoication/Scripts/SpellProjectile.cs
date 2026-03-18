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

    [Header("VFX")]
    public GameObject impactVFX;
    public float vfxDuration = 2f;

    void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

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