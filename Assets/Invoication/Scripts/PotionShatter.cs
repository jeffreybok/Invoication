using UnityEngine;

public class PotionShatter : MonoBehaviour
{
    public GameObject effectPrefab;
    public float shatterVelocity = 2f;
    public float effectDuration = 3f;
    public float effectRadius = 2f;
    public float healAmount = 50f;
    public PotionType potionType;

    public enum PotionType
    {
        Freeze,
        Fire,
        Poison,
        Healing
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > shatterVelocity)
        {
            Shatter(collision.contacts[0].point, collision.gameObject);
        }
    }

    public void Shatter(Vector3 impactPoint, GameObject hitObject)
    {
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

        Destroy(gameObject);
    }

    void ApplyDirectEffect(Enemy enemy)
    {
        switch (potionType)
        {
            case PotionType.Freeze:
                enemy.Freeze(effectDuration);
                break;
            case PotionType.Fire:
                enemy.TakeDamage(50f);
                break;
            case PotionType.Poison:
                // enemy.Poison(effectDuration, 5f);
                break;
            case PotionType.Healing:
                // No effect on enemies
                break;
        }
    }
}