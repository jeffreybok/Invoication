using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Settings")]
    public int currentXP = 0;
    public int currentLevel = 1;
    public int baseXPRequired = 100;
    public float scalingFactor = 1.5f;

    [Header("HUD References")]
    public TextMeshProUGUI xpText;
    public Slider xpBar;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        UpdateHUD();
    }

    // XP needed for next level
    public int GetXPRequired()
    {
        return Mathf.RoundToInt(baseXPRequired * Mathf.Pow(currentLevel, scalingFactor));
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"XP +{amount} → {currentXP}/{GetXPRequired()}");

        while (currentXP >= GetXPRequired())
        {
            currentXP -= GetXPRequired();
            LevelUp();
        }

        UpdateHUD();
    }

    void LevelUp()
    {
        currentLevel++;

        if (playerStats != null)
        {
            playerStats.skillPoints++;
        }

        var manager = FindFirstObjectByType<SkillTreeManager>();

        if (manager != null && GetComponent<PurrNet.NetworkIdentity>().isOwner)
        {
            manager.RefreshSkillPointsDisplay();
        }

        Debug.Log($"LEVEL UP → {currentLevel}");
    }

    void UpdateHUD()
    {
        if (xpText != null)
            xpText.text = $"{currentXP} / {GetXPRequired()} XP";

        if (xpBar != null)
            xpBar.value = (float)currentXP / GetXPRequired();
    }
}