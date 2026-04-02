using UnityEngine;
using PurrNet;

public class EffectZone : NetworkBehaviour
{
    public PotionShatter.PotionType potionType;
    public float radius;
    public float duration;
    public float tickInterval = 0.5f;
    public float healAmount;
    public float fireDamage = 10f;
    public float freezeDuration;

    private GameObject attacker; // who created the zone
    private bool initialized = false;

    public void Initialize(GameObject owner)
    {
        attacker = owner;
        initialized = true;
    }

    void Start()
    {
        if (!isServer) return;

        // Safety check
        if (!initialized)
        {
            Debug.LogWarning("EffectZone spawned without Initialize() — attacker will be null.");
        }

        InvokeRepeating(nameof(ApplyEffect), 0f, tickInterval);

        // Sync destruction across clients
        DestroyZoneAfterTime(duration);
    }

    void DestroyZoneAfterTime(float time)
    {
        Invoke(nameof(DestroyZone), time);
    }

    void DestroyZone()
    {
        DestroyZone_ObserversRPC();
    }

    [ObserversRpc]
    void DestroyZone_ObserversRPC()
    {
        Destroy(gameObject);
    }

    void ApplyEffect()
    {
        if (!isServer) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hitColliders)
        {
            switch (potionType)
            {
                case PotionShatter.PotionType.Freeze:
                {
                    Enemy freezeEnemy = hit.GetComponent<Enemy>();
                    if (freezeEnemy != null && !freezeEnemy.IsFrozen())
                    {
                        freezeEnemy.Freeze(freezeDuration, attacker);
                    }
                    break;
                }

                case PotionShatter.PotionType.Fire:
                {
                    Enemy fireEnemy = hit.GetComponent<Enemy>();
                    if (fireEnemy != null)
                    {
                        fireEnemy.TakeDamage(fireDamage, attacker);
                    }
                    break;
                }

                case PotionShatter.PotionType.Healing:
                {
                    PlayerHealth player = hit.GetComponent<PlayerHealth>();
                    if (player != null)
                    {
                        player.Heal(healAmount);
                    }
                    break;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = potionType == PotionShatter.PotionType.Fire ? Color.red :
                       potionType == PotionShatter.PotionType.Freeze ? Color.cyan :
                       Color.green;

        Gizmos.DrawWireSphere(transform.position, radius);
    }
}