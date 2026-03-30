using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PurrNet;

public class SkillTreeManager : MonoBehaviour
{

    [Header("References")]
    public GameObject skillTreePanel;
    public SkillTreeUI skillTreeUI;
    public TextMeshProUGUI skillPointText;
    public TextMeshProUGUI elementNameText;

    [Header("Skill Trees")]
    public SkillTreeData[] allTrees;
    private int currentTreeIndex = 0;

    private PlayerController playerController;
    private RaycastPickup raycastPickup;
    private PlayerStats stats;
    private PlayerXP xp;

    private bool isOpen = false;



    void Start()
    {
        skillTreePanel.SetActive(false);
        StartCoroutine(InitializeAfterDelay());
    }

    System.Collections.IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        FindOwnedPlayer();

        if (stats == null)
        {
            Debug.Log("[SkillTreeManager] No local player found, skipping init");
            yield break;
        }

        SkillTreeSaveSystem.LoadAll(allTrees, stats, xp);

        foreach (SkillTreeData tree in allTrees)
        {
            foreach (SkillNode node in tree.GetAllNodes())
            {
                if (node.isUnlocked && node.nodeEffect != NodeEffect.None)
                {
                    stats.ApplyEffect(node.nodeEffect, node.effectValue);
                }
            }
        }

        LoadCurrentTree();
    }

    void Update()
    {
        if (stats == null) return; // ✅ only local player can use UI

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen) CloseMenu();
            else OpenMenu();
        }
    }

    void OpenMenu()
    {
        isOpen = true;
        skillTreePanel.SetActive(true);
        RefreshSkillPointsDisplay();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null) playerController.enabled = false;
        if (raycastPickup != null) raycastPickup.enabled = false;
    }

    void CloseMenu()
    {
        isOpen = false;
        skillTreePanel.SetActive(false);

        if (TooltipUI.Instance != null && TooltipUI.Instance.tooltipPanel.activeInHierarchy)
            TooltipUI.Instance.tooltipPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null) playerController.enabled = true;
        if (raycastPickup != null) raycastPickup.enabled = true;
    }

    void LoadCurrentTree()
    {
        if (allTrees == null || allTrees.Length == 0)
            return;

        skillTreeUI.LoadTree(allTrees[currentTreeIndex]);
        elementNameText.text = allTrees[currentTreeIndex].elementName;
        RefreshSkillPointsDisplay();
    }

    public void RefreshSkillPointsDisplay()
    {
        if (stats != null)
            skillPointText.text = $"Skill Points: {stats.skillPoints}";
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

    public bool CanUnlock(SkillTreeData tree, SkillNode node)
    {
        if (node.isUnlocked) return false;

        if (stats.skillPoints < node.skillPointCost)
            return false;

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

        if (node.nodeEffect != NodeEffect.None)
            stats.ApplyEffect(node.nodeEffect, node.effectValue);

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

    void FindOwnedPlayer()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var id = player.GetComponent<NetworkIdentity>();

            if (id != null && id.isOwner)
            {
                stats = player.GetComponent<PlayerStats>();
                xp = player.GetComponent<PlayerXP>();
                playerController = player.GetComponent<PlayerController>();
                raycastPickup = player.GetComponentInChildren<RaycastPickup>();

                Debug.Log("[SkillTreeManager] Found local player");
                break;
            }
        }
    }

    // 🔥 DEBUG: find who disables stuff
    void OnDisable()
    {
        Debug.Log("[SkillTreeManager DISABLED]");
        Debug.Log(System.Environment.StackTrace);
    }
}