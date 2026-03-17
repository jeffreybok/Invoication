using System;
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
    public SkillTreeData[] filteredTrees;
    private int currentTreeIndex = 0;
    
    [Header("Class")]
    public PlayerClass activeClass = PlayerClass.Damage;
    
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
        skillTreePanel.SetActive(false);
        StartCoroutine(InitializeAfterDelay());
    }

    private System.Collections.IEnumerator InitializeAfterDelay()
    {
        // Wait for player to spawn
        yield return new WaitForSeconds(0.5f);
        
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                raycastPickup = player.GetComponentInChildren<RaycastPickup>();
            }
            
        FilterTreesByClass();
        
        SkillTreeSaveSystem.LoadAll(allTrees, out int loadedPoints);
        PlayerStats.Instance.skillPoints = loadedPoints;
        
        foreach (SkillTreeData tree in allTrees)
            foreach (SkillNode node in tree.nodes)
                if (node.isUnlocked && node.nodeEffect != NodeEffect.None)
                    PlayerStats.Instance.ApplyEffect(node.nodeEffect, node.effectValue);
        
        LoadCurrentTree();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen)
                CloseMenu();
            else
                OpenMenu();
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
        
        // Disable player scripts (but NOT camera components)
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
        
        // Re-enable player scripts (but NOT camera components)
        if (playerController != null) playerController.enabled = true;
        if (raycastPickup != null) raycastPickup.enabled = true;
    }

    private void FilterTreesByClass()
    {
        System.Collections.Generic.List<SkillTreeData> matching = new System.Collections.Generic.List<SkillTreeData>();
        
        foreach (SkillTreeData tree in allTrees)
            if (tree.playerClass == activeClass)
                matching.Add(tree);
        
        filteredTrees = matching.ToArray();
    }

    private void LoadCurrentTree()
    {
        if (filteredTrees == null || filteredTrees.Length == 0)
            return;
        
        skillTreeUI.LoadTree(filteredTrees[currentTreeIndex]);
        elementNameText.text = filteredTrees[currentTreeIndex].elementName;
        RefreshSkillPointsDisplay();
    }

    public void RefreshSkillPointsDisplay()
    {
        skillPointText.text = $"Skill Points: {PlayerStats.Instance.skillPoints}";
    }

    public void NextTree()
    {
        currentTreeIndex = (currentTreeIndex + 1) % filteredTrees.Length;
        LoadCurrentTree();
    }

    public void PreviousTree()
    {
        currentTreeIndex = (currentTreeIndex - 1 + filteredTrees.Length) % filteredTrees.Length;
        LoadCurrentTree();
    }
    
    public bool CanUnlock(SkillTreeData tree, int nodeIndex)
    {
        SkillNode node = tree.nodes[nodeIndex];

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

        // Tier 1 node is always unlockable
        if (nodeIndex == 0)
            return true;
        
        // Previous node must be unlocked
        if (!tree.nodes[nodeIndex - 1].isUnlocked)
        {
            Debug.Log($"Previous node must be unlocked first.");
            return false;
        }
        
        return true;
    }

    public void UnlockNode(SkillTreeData tree, int nodeIndex)
    {
        if (!CanUnlock(tree, nodeIndex))
            return;
        
        SkillNode node = tree.nodes[nodeIndex];
        node.isUnlocked = true;
        PlayerStats.Instance.skillPoints -= node.skillPointCost;
        
        if (node.nodeEffect != NodeEffect.None)
            PlayerStats.Instance.ApplyEffect(node.nodeEffect, node.effectValue);
        
        SkillTreeSaveSystem.SaveAll(allTrees, PlayerStats.Instance.skillPoints);
        Debug.Log($"Unlocked {node.nodeName}. Remaining points: {PlayerStats.Instance.skillPoints}");
    }

    public void ResetSkillTree()
    {
        // Calculate total points spent across all unlocked nodes
        int pointsToRefund = 0;
        foreach (SkillTreeData tree in allTrees)
        foreach (SkillNode node in tree.nodes)
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
}
