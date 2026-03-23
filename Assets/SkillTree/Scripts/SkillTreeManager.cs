using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance;
    
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
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Use to reset saved player
        PlayerPrefs.DeleteAll();
        skillTreePanel.SetActive(false);
        StartCoroutine(InitializeAfterDelay());
    }

    private System.Collections.IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        
        FindOwnedPlayer();

        SkillTreeSaveSystem.LoadAll(allTrees, out int loadedPoints);
        PlayerStats.Instance.skillPoints = loadedPoints;

        // Reapply effects for all unlocked nodes across all trees
        foreach (SkillTreeData tree in allTrees)
            foreach (SkillNode node in tree.GetAllNodes())
                if (node.isUnlocked && node.nodeEffect != NodeEffect.None)
                    PlayerStats.Instance.ApplyEffect(node.nodeEffect, node.effectValue);

        LoadCurrentTree();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen) CloseMenu();
            else OpenMenu();
        }
    }

    private void OpenMenu()
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

    private void CloseMenu()
    {
        isOpen = false;
        skillTreePanel.SetActive(false);
        if (TooltipUI.Instance.tooltipPanel.activeInHierarchy)
            TooltipUI.Instance.tooltipPanel.SetActive(false);
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (playerController != null) playerController.enabled = true;
        if (raycastPickup != null) raycastPickup.enabled = true;
    }

    private void LoadCurrentTree()
    {
        if (allTrees == null || allTrees.Length == 0)
            return;
        
        skillTreeUI.LoadTree(allTrees[currentTreeIndex]);
        elementNameText.text = allTrees[currentTreeIndex].elementName;
        RefreshSkillPointsDisplay();
    }

    public void RefreshSkillPointsDisplay()
    {
        skillPointText.text = $"Skill Points: {PlayerStats.Instance.skillPoints}";
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
        if (node.isUnlocked)
        {
            Debug.Log($"{node.nodeName} is already unlocked");
            return false;
        }

        if (PlayerStats.Instance.skillPoints < node.skillPointCost)
        {
            Debug.Log($"Not enough skill points for {node.nodeName}");
            return false;
        }

        // No prerequisites means Tier 1 - always unlockable
        if (node.prerequisiteNodeIDs == null || node.prerequisiteNodeIDs.Count == 0)
            return true;

        // All prerequisites must be unlocked
        List<SkillNode> allNodes = tree.GetAllNodes();
        foreach (string prereqID in node.prerequisiteNodeIDs)
        {
            SkillNode prereqNode = allNodes.Find(n => n.nodeID == prereqID);
            if (prereqNode == null || !prereqNode.isUnlocked)
            {
                Debug.Log($"Prerequisite {prereqID} not unlocked for {node.nodeName}");
                return false;
            }
        }

        return true;
    }

    public void UnlockNode(SkillTreeData tree, SkillNode node)
    {
        if (!CanUnlock(tree, node))
            return;

        node.isUnlocked = true;
        PlayerStats.Instance.skillPoints -= node.skillPointCost;

        if (node.nodeEffect != NodeEffect.None)
            PlayerStats.Instance.ApplyEffect(node.nodeEffect, node.effectValue);

        SkillTreeSaveSystem.SaveAll(allTrees, PlayerStats.Instance.skillPoints);
        Debug.Log($"Unlocked {node.nodeName}. Remaining points: {PlayerStats.Instance.skillPoints}");
    }

    public void ResetSkillTree()
    {
        int pointsToRefund = 0;
        foreach (SkillTreeData tree in allTrees)
            foreach (SkillNode node in tree.GetAllNodes())
                if (node.isUnlocked)
                    pointsToRefund += node.skillPointCost;

        int newTotal = PlayerStats.Instance.skillPoints + pointsToRefund;

        SkillTreeSaveSystem.ResetAll(allTrees, newTotal, out int resetPoints);
        PlayerStats.Instance.skillPoints = resetPoints;
        PlayerStats.Instance.ResetToBase();
        LoadCurrentTree();
        RefreshSkillPointsDisplay();
        Debug.Log("Skill tree reset.");
    }

    private void FindOwnedPlayer()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerCameraOwner cameraOwner = player.GetComponent<PlayerCameraOwner>();
            if (cameraOwner != null && cameraOwner.isOwner)
            {
                playerController = player.GetComponent<PlayerController>();
                raycastPickup = player.GetComponentInChildren<RaycastPickup>();
                break;
            }
        }
    }
}