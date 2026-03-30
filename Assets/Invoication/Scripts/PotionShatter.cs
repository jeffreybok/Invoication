using UnityEngine;
using PurrNet;

public class PotionShatter : NetworkBehaviour
{
    public GameObject effectPrefab;
    public float shatterVelocity = 2f;
    public float effectDuration = 3f;
    public float effectRadius = 2f;
    public float healAmount = 50f;
    public PotionType potionType;

    private GameObject attacker;

    public enum PotionType
    {
        Freeze,
        Fire,
        Poison,
        Healing
    }

    public void Initialize(GameObject owner)
    {
        attacker = owner;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        if (collision.relativeVelocity.magnitude > shatterVelocity)
        {
            Shatter(collision.contacts[0].point, collision.gameObject);
        }
    }

    public void Shatter(Vector3 impactPoint, GameObject hitObject)
    {
        if (!isServer) return;

        Enemy enemy = hitObject.GetComponent<Enemy>();
        if (enemy != null)
            ApplyDirectEffect(enemy);

        Vector3 spawnPos = impactPoint;
        if (Physics.Raycast(impactPoint, Vector3.down, out RaycastHit hit, 100f))
            spawnPos = hit.point;

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        GameObject zone = new GameObject("EffectZone");
        zone.transform.position = spawnPos;

        EffectZone effectZone = zone.AddComponent<EffectZone>();
        effectZone.potionType = potionType;
        effectZone.radius = effectRadius;
        effectZone.duration = effectDuration;
        effectZone.healAmount = healAmount;
        effectZone.freezeDuration = effectDuration;

        effectZone.Initialize(attacker);

        Destroy(zone, effectDuration);
        Destroy(gameObject);
    }

    void ApplyDirectEffect(Enemy enemy)
    {
        switch (potionType)
        {
            case PotionType.Freeze:
                enemy.Freeze_Server(effectDuration, attacker);
                break;

            case PotionType.Fire:
                enemy.TakeDamage_Server(50f, attacker);
                break;

            case PotionType.Poison:
                // enemy.Poison_Server(...)
                break;

            case PotionType.Healing:
                break;
        }
    }
}