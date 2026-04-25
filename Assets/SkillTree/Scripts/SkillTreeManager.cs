using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PurrNet;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour
{
    [Header("References")]
    public GameObject skillTreePanel;
    public SkillTreeUI skillTreeUI;
    public TooltipUI tooltipUI;
    public ConfirmationUI confirmationUI;

    public TextMeshProUGUI skillPointText;
    public Image elementTitleImage;
    public Image leftArrowImage;
    public Image rightArrowImage;

    [Header("Skill Trees")]
    public SkillTreeData[] allTrees;
    private int currentTreeIndex = 0;

    private PlayerController playerController;
    private PlayerStats stats;
    private PlayerXP xp;

    private bool isOpen = false;

    void Start()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);

        StartCoroutine(InitializeAfterDelay());
    }

    private System.Collections.IEnumerator InitializeAfterDelay()
    {
        // wait for PurrNet player spawn
        yield return new WaitForSeconds(2f);

        if (!FindOwnedPlayer())
        {
            Debug.Log("[SkillTreeManager] Not owner → disabling UI");
            if (skillTreePanel != null)
                skillTreePanel.SetActive(false);
            enabled = false;
            yield break;
        }

        // 🔥 ALWAYS re-fetch UI from player (handles inactive + prefab cases)
        skillTreeUI = playerController.GetComponentInChildren<SkillTreeUI>(true);
        tooltipUI = playerController.GetComponentInChildren<TooltipUI>(true);
        confirmationUI = playerController.GetComponentInChildren<ConfirmationUI>(true);

        // 🔥 assign manager safely
        if (skillTreeUI != null)
        {
            skillTreeUI.manager = this;
        }
        else
        {
            Debug.LogError("SkillTreeUI NULL");
        }

        if (confirmationUI != null)
        {
            confirmationUI.Initialize(this);
        }
        else
        {
            Debug.LogError("ConfirmationUI NULL AFTER SPAWN");
        }

        // load data
        SkillTreeSaveSystem.LoadAll(allTrees, stats, xp);

        LoadCurrentTree();
    }

    bool FindOwnedPlayer()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var id = player.GetComponent<NetworkIdentity>();

            if (id != null && id.isOwner)
            {
                stats = player.GetComponent<PlayerStats>();
                xp = player.GetComponent<PlayerXP>();
                playerController = player.GetComponent<PlayerController>();

                Debug.Log("[SkillTreeManager] Found local player");
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        if (stats == null) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen) CloseMenu();
            else OpenMenu();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetSkillTree();
        }
    }

    void OpenMenu()
    {
        isOpen = true;

        if (skillTreePanel != null)
            skillTreePanel.SetActive(true);

        RefreshSkillPointsDisplay();

        if (playerController != null)
            playerController.lockCamera = true;

        HookAllButtons();

        // 🔥 ADD THIS LINE
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBook();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseMenu()
    {
        isOpen = false;

        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);

        if (playerController != null)
            playerController.lockCamera = false;

        // 🔥 SAFE CALL
        if (tooltipUI != null)
            tooltipUI.Hide();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LoadCurrentTree()
    {
        if (allTrees == null || allTrees.Length == 0) return;
        if (skillTreeUI == null) return;

        skillTreeUI.LoadTree(allTrees[currentTreeIndex]);

        if (elementTitleImage != null && allTrees[currentTreeIndex].elementIcon != null)
            elementTitleImage.sprite = allTrees[currentTreeIndex].elementIcon;

        RefreshSkillPointsDisplay();
    }

    public void RefreshSkillPointsDisplay()
    {
        if (stats != null && skillPointText != null)
            skillPointText.text = $"{stats.skillPoints}";
    }

    public bool CanUnlock(SkillTreeData tree, SkillNode node)
    {
        if (node.isUnlocked) return false;
        if (stats.skillPoints < node.skillPointCost) return false;

        if (node.prerequisiteNodeIDs == null || node.prerequisiteNodeIDs.Count == 0)
            return true;

        List<SkillNode> allNodes = tree.GetAllNodes();

        foreach (string prereqID in node.prerequisiteNodeIDs)
        {
            SkillNode prereqNode = allNodes.Find(n => n.nodeID == prereqID);
            if (prereqNode == null || !prereqNode.isUnlocked)
                return false;
        }

        return true;
    }

    public void UnlockNode(SkillTreeData tree, SkillNode node)
    {
        if (!CanUnlock(tree, node)) return;

        node.isUnlocked = true;
        stats.skillPoints -= node.skillPointCost;

        SkillTreeSaveSystem.SaveAll(allTrees, stats, xp);
        RefreshSkillPointsDisplay();
    }

    public void ResetSkillTree()
    {
        int refund = 0;

        foreach (SkillTreeData tree in allTrees)
            foreach (SkillNode node in tree.GetAllNodes())
                if (node.isUnlocked)
                    refund += node.skillPointCost;

        int newTotal = stats.skillPoints + refund;

        SkillTreeSaveSystem.ResetAll(allTrees, newTotal, stats, xp);
        stats.ResetToBase();

        LoadCurrentTree();
        RefreshSkillPointsDisplay();
    }

    void HookAllButtons()
    {
        if (skillTreePanel == null) return;

        Button[] buttons = skillTreePanel.GetComponentsInChildren<Button>(true);

        foreach (Button btn in buttons)
        {
            // ❌ SKIP confirm button (it has its own sound)
            if (btn.name.Contains("Confirm"))
                continue;

            btn.onClick.RemoveListener(PlaySelectSound);
            btn.onClick.AddListener(PlaySelectSound);
        }
    }
    
    public void NextTree()
    {
        currentTreeIndex = (currentTreeIndex + 1) % allTrees.Length;
        LoadCurrentTree();
    }

    public void PreviousTree()
    {
        currentTreeIndex = (currentTreeIndex - 1 + allTrees.Length) % allTrees.Length;
        LoadCurrentTree();
    }

    void PlaySelectSound()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySelect();
    }
}