using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player")]
    public float baseMana = 100f;
    public float currentMana;
    public int skillPoints;

    [Header("Fireball Stats")]
    public float fireballBaseDamage = 50f;
    public float fireballFlatBonus = 0f;
    public float fireballExplosionRadius = 5f;

    [Header("Blazing Impact Stats")]
    public float blazingBaseDamage = 30f;
    public float blazingFlatBonus = 0f;
    public float blazingBurnDamagePerTick = 5f;
    public float blazingBurnDuration = 3f;

    [Header("Ice Spike Stats")]
    public float iceSpikeBaseDamage = 30f;
    public float iceSpikeFlatBonus = 0f;
    public float iceSpikeSlowDuration = 2f;
    public float iceSpikeFreezeRadius = 2f;

    void Start()
    {
        currentMana = baseMana;
    }

    // --- Getters ---

    public float GetFireballDamage()
    {
        return fireballBaseDamage + fireballFlatBonus;
    }

    public float GetBlazingDamage()
    {
        return blazingBaseDamage + blazingFlatBonus;
    }

    public float GetIceSpikeDamage()
    {
        return iceSpikeBaseDamage + iceSpikeFlatBonus;
    }

    // --- Reset ---

    public void ResetToBase()
    {
        currentMana = baseMana;

        fireballFlatBonus = 0f;
        fireballExplosionRadius = 5f;

        blazingFlatBonus = 0f;
        blazingBurnDamagePerTick = 5f;
        blazingBurnDuration = 3f;

        iceSpikeFlatBonus = 0f;
        iceSpikeSlowDuration = 2f;
        iceSpikeFreezeRadius = 2f;
    }

    // --- Apply skill effects ---

    public void ApplyEffect(NodeEffect effect, float value)
    {
        switch (effect)
        {
            case NodeEffect.FireballFlatDamage:
                fireballFlatBonus += value;
                Debug.Log($"Fireball +{value} → {GetFireballDamage()}");
                break;

            case NodeEffect.FireballExplosionRadius:
                fireballExplosionRadius += value;
                Debug.Log($"Fireball radius → {fireballExplosionRadius}");
                break;

            case NodeEffect.BlazingFlatDamage:
                blazingFlatBonus += value;
                Debug.Log($"Blazing +{value} → {GetBlazingDamage()}");
                break;

            case NodeEffect.BlazingBurnDamage:
                blazingBurnDamagePerTick += value;
                Debug.Log($"Burn dmg → {blazingBurnDamagePerTick}");
                break;

            case NodeEffect.BlazingBurnDuration:
                blazingBurnDuration += value;
                Debug.Log($"Burn duration → {blazingBurnDuration}");
                break;

            case NodeEffect.IceSpikeFlatDamage:
                iceSpikeFlatBonus += value;
                Debug.Log($"Ice +{value} → {GetIceSpikeDamage()}");
                break;

            case NodeEffect.IceSpikeSlowDuration:
                iceSpikeSlowDuration += value;
                Debug.Log($"Slow duration → {iceSpikeSlowDuration}");
                break;

            case NodeEffect.IceSpikeFreezeRadius:
                iceSpikeFreezeRadius += value;
                Debug.Log($"Freeze radius → {iceSpikeFreezeRadius}");
                break;
        }
    }
}