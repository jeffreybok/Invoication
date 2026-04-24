using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DevToolPanel : MonoBehaviour
{
    public GameObject panel;
    public KeyCode toggleKey = KeyCode.F1;

    public TextMeshProUGUI currentStatsText;

    public TMP_InputField skillPointsInput;
    public Button setSkillPointsButton;

    public TMP_InputField xpInput;
    public Button giveXPButton;

    public Button levelMinusButton;
    public Button levelPlusButton;

    public Button resetTreeButton;

    private bool isOpen = false;

    private PlayerStats stats;
    private PlayerXP xp;

    void Start()
    {
        panel.SetActive(false);

        FindLocalPlayer();

        setSkillPointsButton.onClick.AddListener(OnSetSkillPoints);
        giveXPButton.onClick.AddListener(OnGiveXP);
        levelMinusButton.onClick.AddListener(OnLevelMinus);
        levelPlusButton.onClick.AddListener(OnLevelPlus);
        resetTreeButton.onClick.AddListener(OnResetTree);
    }

    void FindLocalPlayer()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var id = player.GetComponent<PurrNet.NetworkIdentity>();
            if (id != null && id.isOwner)
            {
                stats = player.GetComponent<PlayerStats>();
                xp = player.GetComponent<PlayerXP>();
                break;
            }
        }
    }

    /* void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) ClosePanel();
            else OpenPanel();
        }
    } */

    void OpenPanel()
    {
        isOpen = true;
        panel.SetActive(true);
        UpdateStatsDisplay();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ClosePanel()
    {
        isOpen = false;
        panel.SetActive(false);

        Time.timeScale = 1f;
    }

    void UpdateStatsDisplay()
    {
        if (stats == null || xp == null) return;

        currentStatsText.text =
            $"Skill Points: {stats.skillPoints}\n" +
            $"Level: {xp.currentLevel}\n" +
            $"XP: {xp.currentXP} / {xp.GetXPRequired()}";
    }

    void OnSetSkillPoints()
    {
        if (int.TryParse(skillPointsInput.text, out int amount))
        {
            stats.skillPoints = amount;
            UpdateStatsDisplay();
        }
    }

    void OnGiveXP()
    {
        if (int.TryParse(xpInput.text, out int amount))
        {
            xp.GainXP(amount);
            UpdateStatsDisplay();
        }
    }

    void OnLevelMinus()
    {
        if (xp.currentLevel <= 1) return;

        xp.currentLevel--;
        xp.currentXP = 0;
        UpdateStatsDisplay();
    }

    void OnLevelPlus()
    {
        xp.currentLevel++;
        xp.currentXP = 0;
        UpdateStatsDisplay();
    }

    void OnResetTree()
    {
        var manager = FindFirstObjectByType<SkillTreeManager>();

        if (manager != null)
        {
            manager.ResetSkillTree();
        }

        UpdateStatsDisplay();
    }
}