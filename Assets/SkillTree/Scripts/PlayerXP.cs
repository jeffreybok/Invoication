using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance;

    [Header("XP Settings")]
    public int currentXP = 0;
    public int currentLevel = 1;
    public int baseXPRequired = 100;
    public float scalingFactor = 1.5f;

    [Header("HUD References")]
    // public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public Slider xpBar;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateHUD();
    }

    // Returns XP required to reach next level from current level
    public int GetXPRequired()
    {
        return Mathf.RoundToInt(baseXPRequired * Mathf.Pow(currentLevel, scalingFactor));
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"Gained {amount} XP. Total: {currentXP}/{GetXPRequired()}");

        // Handle multiple level ups in one kill
        while (currentXP >= GetXPRequired())
        {
            currentXP -= GetXPRequired();
            LevelUp();
        }

        UpdateHUD();
    }

    private void LevelUp()
    {
        currentLevel++;
        PlayerStats.Instance.skillPoints++;
        SkillTreeManager.Instance.RefreshSkillPointsDisplay();

        Debug.Log($"Level Up! Now level {currentLevel}. Skill points: {PlayerStats.Instance.skillPoints}");
    }

    private void UpdateHUD()
    {
        // if (levelText != null)
            // levelText.text = $"Level {currentLevel}";

        if (xpText != null)
            xpText.text = $"{currentXP} / {GetXPRequired()} XP";

        if (xpBar != null)
            xpBar.value = (float)currentXP / GetXPRequired();
    }
}