using UnityEngine;

public class EffectZone : MonoBehaviour
{
    public PotionShatter.PotionType potionType;
    public float radius;
    public float duration;
    public float tickInterval = 0.5f;
    public float healAmount;
    public float fireDamage = 10f;
    public float freezeDuration;

    void Start()
    {
        InvokeRepeating(nameof(ApplyEffect), 0f, tickInterval);
        Destroy(gameObject, duration);
    }

    void ApplyEffect()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hitColliders)
        {
            switch (potionType)
            {
                case PotionShatter.PotionType.Freeze:
                    Enemy freezeEnemy = hit.GetComponent<Enemy>();
                    if (freezeEnemy != null && !freezeEnemy.IsFrozen())
                        freezeEnemy.Freeze(freezeDuration);
                    break;

                case PotionShatter.PotionType.Fire:
                    Enemy fireEnemy = hit.GetComponent<Enemy>();
                    if (fireEnemy != null)
                        fireEnemy.TakeDamage(fireDamage);
                    break;

                case PotionShatter.PotionType.Healing:
                    PlayerHealth player = hit.GetComponent<PlayerHealth>();
                    if (player != null)
                        player.Heal(healAmount);
                    break;
            }
        }
    }
}