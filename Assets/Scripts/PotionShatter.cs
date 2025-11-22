using UnityEngine;

public class PotionShatter : MonoBehaviour
{
    public GameObject effectPrefab;
    public float shatterVelocity = 2f;
    public float effectDuration = 3f;
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
    
    void Shatter(Vector3 impactPoint, GameObject hitObject)
    {
        Enemy enemy = hitObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            ApplyDirectEffect(enemy);
        }
        
        if (effectPrefab != null)
        {
            Vector3 spawnPos = impactPoint;
            
            if (Physics.Raycast(impactPoint, Vector3.down, out RaycastHit hit, 100f))
            {
                spawnPos = hit.point;
            }
            
            GameObject effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);
            InitializeEffect(effect);
            Destroy(effect, effectDuration);
        }
        
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
    
    void InitializeEffect(GameObject effect)
    {
        switch (potionType)
        {
            case PotionType.Freeze:
                FreezeZone freezeZone = effect.GetComponent<FreezeZone>();
                if (freezeZone != null)
                {
                    freezeZone.freezeDuration = effectDuration;
                }
                break;
            case PotionType.Fire:
                // TODO: Add FireZone script
                break;
            case PotionType.Poison:
                // TODO: Add PoisonZone script
                break;
            case PotionType.Healing:
                // TODO: Add HealingZone script
                break;
        }
    }
}