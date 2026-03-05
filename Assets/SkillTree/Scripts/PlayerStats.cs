using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    
    [Header("Base Stats")]
    public int skillPoints = 0;
    public float baseDamage = 10f;
    public float baseBurnDuration = 2f;
    public float baseFireRate = 1f;
    public float baseRange = 5f;
    
    [Header("Current Stats")]
    public float currentDamage;
    public float currentBurnDuration;
    public float currentFireRate;
    public float currentRange;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        ResetToBase();
    }

    public void ResetToBase()
    {
        currentDamage = baseDamage;
        currentBurnDuration = baseBurnDuration;
        currentFireRate = baseFireRate;
        currentRange = baseRange;
    }

    public void ApplyEffect(NodeEffect effect, float value)
    {
        switch (effect)
        {
            case NodeEffect.IncreaseDamage:
                currentDamage += value;
                Debug.Log($"Damage increased to {currentDamage}");
                break;
            case NodeEffect.IncreaseBurnDuration:
                currentBurnDuration += value;
                Debug.Log($"Burn duration increased to {currentBurnDuration}");
                break;
            case NodeEffect.IncreaseFireRate:
                currentFireRate += value;
                Debug.Log($"Fire rate increased to {currentFireRate}");
                break;
            case NodeEffect.IncreaseRange:
                currentRange += value;
                Debug.Log($"Range increased to {currentRange}");
                break;
        }
    }
}
