using UnityEngine;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Node Prefabs")]
    public GameObject majorNodePrefab;
    public GameObject minorNodePrefab;
    
    [Header("Class Container")]
    public Transform tankContainer;
    public Transform damageContainer;
    public Transform supportContainer;
    
    [Header("Line Containers")]
    public Transform tankLineContainer;
    public Transform damageLineContainer;
    public Transform supportLineContainer;
    
    [Header("Class Icons")]
    public UnityEngine.UI.Image tankIcon;
    public UnityEngine.UI.Image damageIcon;
    public UnityEngine.UI.Image supportIcon;
    
    [Header("Vertical Spacing")]
    public float tier1Y = 100f;
    public float tier2Y = 0f;
    public float tier3Y = 0f;
    public float subUpgradeY = -120f;
    public float tier2X = -80f;
    public float tier3X = 80f;
    public float subSpacingX = 100f;

    private SkillTreeData currentTree;
    private List<SkillNodeUI> spawnedNodes = new List<SkillNodeUI>();
    private Dictionary<string, RectTransform> nodeRectMap = new Dictionary<string, RectTransform>();

    public void LoadTree(SkillTreeData tree)
    {
        currentTree = tree;
        spawnedNodes.Clear();
        nodeRectMap.Clear();

        ClearContainer(tankContainer);
        ClearContainer(damageContainer);
        ClearContainer(supportContainer);
        
        SpawnClassTree(tree.tankTree, tankContainer, tankLineContainer);
        SpawnClassTree(tree.damageTree, damageContainer, damageLineContainer);
        SpawnClassTree(tree.supportTree, supportContainer, supportLineContainer);
        
        if (tankIcon != null && tree.tankTree.classIcon != null)
            tankIcon.sprite = tree.tankTree.classIcon;
        if (damageIcon != null && tree.damageTree.classIcon != null)
            damageIcon.sprite = tree.damageTree.classIcon;
        if (supportIcon != null && tree.supportTree.classIcon != null)
            supportIcon.sprite = tree.supportTree.classIcon;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            if (child.gameObject.name != "LineContainer")
                Destroy(child.gameObject);
        }
    }
    
    private void SpawnClassTree(SkillClassTree classTree, Transform container, Transform lineContainer)
    {
        if (classTree == null || classTree.nodes == null) return;

        // Categorize nodes by their role
        List<SkillNode> tier1Nodes = new List<SkillNode>();
        List<SkillNode> tier2Nodes = new List<SkillNode>();
        List<SkillNode> tier3Nodes = new List<SkillNode>();
        List<SkillNode> subNodes = new List<SkillNode>();

        foreach (SkillNode node in classTree.nodes)
        {
            if (node.prerequisiteNodeIDs == null || node.prerequisiteNodeIDs.Count == 0)
                tier1Nodes.Add(node);
            else if (node.nodeType == NodeType.Minor)
                subNodes.Add(node);
            else if (tier2Nodes.Count == 0)
                tier2Nodes.Add(node);
            else
                tier3Nodes.Add(node);
        }

        // Spawn Tier 1
        foreach (SkillNode node in tier1Nodes)
            SpawnNode(node, currentTree, container, new Vector2(0, tier1Y));

        // Spawn Tier 2
        foreach (SkillNode node in tier2Nodes)
            SpawnNode(node, currentTree, container, new Vector2(tier2X, tier2Y));

        // Spawn Tier 3
        foreach (SkillNode node in tier3Nodes)
            SpawnNode(node, currentTree, container, new Vector2(tier3X, tier3Y));

        // Spawn sub-upgrades - split evenly under their parent tier
        List<SkillNode> tier2Subs = new List<SkillNode>();
        List<SkillNode> tier3Subs = new List<SkillNode>();

        foreach (SkillNode sub in subNodes)
        {
            bool underTier2 = tier2Nodes.Count > 0 &&
                sub.prerequisiteNodeIDs.Contains(tier2Nodes[0].nodeID);
            if (underTier2)
                tier2Subs.Add(sub);
            else
                tier3Subs.Add(sub);
        }

        for (int i = 0; i < tier2Subs.Count; i++)
        {
            float xOffset = tier2X + (i - (tier2Subs.Count - 1) / 2f) * subSpacingX;
            SpawnNode(tier2Subs[i], currentTree, container, new Vector2(xOffset, subUpgradeY));
        }

        for (int i = 0; i < tier3Subs.Count; i++)
        {
            float xOffset = tier3X + (i - (tier3Subs.Count - 1) / 2f) * subSpacingX;
            SpawnNode(tier3Subs[i], currentTree, container, new Vector2(xOffset, subUpgradeY));
        }

        // Draw lines based on prerequisites
        DrawTreeLines(classTree, lineContainer);
    }
    
    private void SpawnNode(SkillNode node, SkillTreeData tree, Transform container, Vector2 position)
    {
        GameObject prefab = node.nodeType == NodeType.Major ? majorNodePrefab : minorNodePrefab;
        GameObject spawned = Instantiate(prefab, container);

        RectTransform rt = spawned.GetComponent<RectTransform>();
        rt.anchoredPosition = position;

        SkillNodeUI nodeUI = spawned.GetComponent<SkillNodeUI>();
        nodeUI.Initialize(node, tree);
        spawnedNodes.Add(nodeUI);
        nodeRectMap[node.nodeID] = rt;
    }
    
    private void DrawTreeLines(SkillClassTree classTree, Transform lineContainer)
    {
        // Clear old lines
        foreach (Transform child in lineContainer)
            Destroy(child.gameObject);

        foreach (SkillNode node in classTree.nodes)
        {
            if (node.prerequisiteNodeIDs == null) continue;

            foreach (string prereqID in node.prerequisiteNodeIDs)
            {
                if (nodeRectMap.ContainsKey(node.nodeID) && nodeRectMap.ContainsKey(prereqID))
                {
                    NodeConnector.Instance.DrawLine(
                        nodeRectMap[prereqID],
                        nodeRectMap[node.nodeID],
                        lineContainer,
                        node.isUnlocked
                    );
                }
            }
        }
    }
    
    public void RefreshAllNodes()
    {
        foreach (var node in spawnedNodes)
            node.UpdateVisual();

        if (currentTree == null) return;
        DrawTreeLines(currentTree.tankTree, tankLineContainer);
        DrawTreeLines(currentTree.damageTree, damageLineContainer);
        DrawTreeLines(currentTree.supportTree, supportLineContainer);
    }
}