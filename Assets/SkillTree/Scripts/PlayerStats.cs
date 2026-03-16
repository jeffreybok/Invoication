using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentMana = baseMana;
    }

    // --- Getters so spells always read the correct final value ---

    public float GetFireballDamage()
    {
        return (fireballBaseDamage + fireballFlatBonus);
    }

    public float GetBlazingDamage()
    {
        return (blazingBaseDamage + blazingFlatBonus);
    }

    public float GetIceSpikeDamage()
    {
        return (iceSpikeBaseDamage + iceSpikeFlatBonus);
    }

    // --- Called by SkillTreeManager when a node is unlocked ---

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

    public void ApplyEffect(NodeEffect effect, float value)
    {
        switch (effect)
        {
            // Fireball
            case NodeEffect.FireballFlatDamage:
                fireballFlatBonus += value;
                Debug.Log($"Fireball flat damage +{value}. Total: {GetFireballDamage()}");
                break;
            case NodeEffect.FireballExplosionRadius:
                fireballExplosionRadius += value;
                Debug.Log($"Fireball radius +{value}. Total: {fireballExplosionRadius}");
                break;

            // Blazing Impact
            case NodeEffect.BlazingFlatDamage:
                blazingFlatBonus += value;
                Debug.Log($"Blazing flat damage +{value}. Total: {GetBlazingDamage()}");
                break;
            case NodeEffect.BlazingBurnDamage:
                blazingBurnDamagePerTick += value;
                Debug.Log($"Blazing burn damage +{value}. Total: {blazingBurnDamagePerTick}");
                break;
            case NodeEffect.BlazingBurnDuration:
                blazingBurnDuration += value;
                Debug.Log($"Blazing burn duration +{value}. Total: {blazingBurnDuration}");
                break;

            // Ice Spike
            case NodeEffect.IceSpikeFlatDamage:
                iceSpikeFlatBonus += value;
                Debug.Log($"Ice spike flat damage +{value}. Total: {GetIceSpikeDamage()}");
                break;
            case NodeEffect.IceSpikeSlowDuration:
                iceSpikeSlowDuration += value;
                Debug.Log($"Ice spike slow duration +{value}. Total: {iceSpikeSlowDuration}");
                break;
            case NodeEffect.IceSpikeFreezeRadius:
                iceSpikeFreezeRadius += value;
                Debug.Log($"Ice spike freeze radius +{value}. Total: {iceSpikeFreezeRadius}");
                break;
        }
    }
}
