using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DevToolPanel : MonoBehaviour
{
    public static DevToolPanel Instance;

    [Header("Panel")]
    public GameObject panel;
    public KeyCode toggleKey = KeyCode.F1;

    [Header("Stats Display")]
    public TextMeshProUGUI currentStatsText;

    [Header("Skill Points")]
    public TMP_InputField skillPointsInput;
    public Button setSkillPointsButton;

    [Header("XP")]
    public TMP_InputField xpInput;
    public Button giveXPButton;

    [Header("Level")]
    public Button levelMinusButton;
    public Button levelPlusButton;

    [Header("Reset")]
    public Button resetTreeButton;

    private bool isOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        panel.SetActive(false);

        setSkillPointsButton.onClick.AddListener(OnSetSkillPoints);
        giveXPButton.onClick.AddListener(OnGiveXP);
        levelMinusButton.onClick.AddListener(OnLevelMinus);
        levelPlusButton.onClick.AddListener(OnLevelPlus);
        resetTreeButton.onClick.AddListener(OnResetTree);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) ClosePanel();
            else OpenPanel();
        }
    }

    private void OpenPanel()
    {
        if (SkillTreeManager.Instance != null &&
            SkillTreeManager.Instance.skillTreePanel.activeInHierarchy)
        {
            Debug.LogWarning("[DevTool] Close skill tree before opening Dev Tools");
            return;
        }

        isOpen = true;
        panel.SetActive(true);
        UpdateStatsDisplay();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ClosePanel()
    {
        isOpen = false;
        panel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateStatsDisplay()
    {
        if (currentStatsText == null) return;
        currentStatsText.text =
            $"Skill Points: {PlayerStats.Instance.skillPoints}\n" +
            $"Level: {PlayerXP.Instance.currentLevel}\n" +
            $"XP: {PlayerXP.Instance.currentXP} / {PlayerXP.Instance.GetXPRequired()}";
    }

    private void OnSetSkillPoints()
    {
        if (int.TryParse(skillPointsInput.text, out int amount))
        {
            PlayerStats.Instance.skillPoints = amount;
            SkillTreeSaveSystem.SaveAll(
                SkillTreeManager.Instance.allTrees,
                PlayerStats.Instance.skillPoints
            );
            SkillTreeManager.Instance.RefreshSkillPointsDisplay();
            UpdateStatsDisplay();
            Debug.Log($"[DevTool] Skill points set to {amount}");
        }
        else
        {
            Debug.LogWarning("[DevTool] Invalid skill points value");
        }
    }

    private void OnGiveXP()
    {
        if (int.TryParse(xpInput.text, out int amount))
        {
            PlayerXP.Instance.GainXP(amount);
            SkillTreeSaveSystem.SaveAll(
                SkillTreeManager.Instance.allTrees,
                PlayerStats.Instance.skillPoints
            );
            UpdateStatsDisplay();
            Debug.Log($"[DevTool] Gave {amount} XP");
        }
        else
        {
            Debug.LogWarning("[DevTool] Invalid XP value");
        }
    }

    private void OnLevelMinus()
    {
        if (PlayerXP.Instance.currentLevel <= 1)
        {
            Debug.LogWarning("[DevTool] Already at minimum level");
            return;
        }

        PlayerXP.Instance.currentLevel--;
        PlayerXP.Instance.currentXP = 0;
        SkillTreeSaveSystem.SaveAll(
            SkillTreeManager.Instance.allTrees,
            PlayerStats.Instance.skillPoints
        );
        UpdateStatsDisplay();
        Debug.Log($"[DevTool] Level decreased to {PlayerXP.Instance.currentLevel}");
    }

    private void OnLevelPlus()
    {
        PlayerXP.Instance.currentLevel++;
        PlayerXP.Instance.currentXP = 0;
        SkillTreeSaveSystem.SaveAll(
            SkillTreeManager.Instance.allTrees,
            PlayerStats.Instance.skillPoints
        );
        UpdateStatsDisplay();
        Debug.Log($"[DevTool] Level increased to {PlayerXP.Instance.currentLevel}");
    }

    private void OnResetTree()
    {
        SkillTreeManager.Instance.ResetSkillTree();
        UpdateStatsDisplay();
        Debug.Log("[DevTool] Skill tree reset");
    }
}