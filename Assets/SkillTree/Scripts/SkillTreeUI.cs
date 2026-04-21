using UnityEngine;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Node Prefabs")]
    public GameObject nodePrefab;

    [Header("Class Containers")]
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
    public float tier2X = -80f;
    public float tier3X = 80f;

    private SkillTreeData currentTree;
    private List<SkillNodeUI> spawnedNodes = new List<SkillNodeUI>();
    private Dictionary<string, RectTransform> nodeRectMap = new Dictionary<string, RectTransform>();
    
    [SerializeField] private NodeConnector nodeConnector;
    
    void Start()
    {
        if (nodeConnector == null)
            nodeConnector = GetComponentInParent<NodeConnector>();
    }

    public void LoadTree(SkillTreeData tree)
    {
        currentTree = tree;
        spawnedNodes.Clear();
        nodeRectMap.Clear();

        ClearContainer(tankContainer);
        ClearContainer(damageContainer);
        ClearContainer(supportContainer);

        SpawnClassTree(tree.tankTree, tankContainer);
        SpawnClassTree(tree.damageTree, damageContainer);
        SpawnClassTree(tree.supportTree, supportContainer);

        DrawTreeLines(tree.tankTree, tankLineContainer);
        DrawTreeLines(tree.damageTree, damageLineContainer);
        DrawTreeLines(tree.supportTree, supportLineContainer);

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

    private void SpawnClassTree(SkillClassTree classTree, Transform container)
    {
        if (classTree == null || classTree.nodes == null) return;

        List<SkillNode> tier1Nodes = new List<SkillNode>();
        List<SkillNode> tier2Nodes = new List<SkillNode>();
        List<SkillNode> tier3Nodes = new List<SkillNode>();

        foreach (SkillNode node in classTree.nodes)
        {
            if (node.prerequisiteNodeIDs == null || node.prerequisiteNodeIDs.Count == 0)
                tier1Nodes.Add(node);
            else if (tier2Nodes.Count == 0)
                tier2Nodes.Add(node);
            else
                tier3Nodes.Add(node);
        }

        foreach (SkillNode node in tier1Nodes)
            SpawnNode(node, currentTree, container, new Vector2(0, tier1Y));

        foreach (SkillNode node in tier2Nodes)
            SpawnNode(node, currentTree, container, new Vector2(tier2X, tier2Y));

        foreach (SkillNode node in tier3Nodes)
            SpawnNode(node, currentTree, container, new Vector2(tier3X, tier3Y));
    }

    private void SpawnNode(SkillNode node, SkillTreeData tree, Transform container, Vector2 position)
    {
        GameObject spawned = Instantiate(nodePrefab, container);
        RectTransform rt = spawned.GetComponent<RectTransform>();
        rt.anchoredPosition = position;

        SkillNodeUI nodeUI = spawned.GetComponent<SkillNodeUI>();
        nodeUI.Initialize(node, tree);
        spawnedNodes.Add(nodeUI);
        nodeRectMap[node.nodeID] = rt;
    }

    private void DrawTreeLines(SkillClassTree classTree, Transform lineContainer)
    {
        if (lineContainer == null || classTree == null || classTree.nodes == null) return;

        foreach (Transform child in lineContainer)
            Destroy(child.gameObject);

        foreach (SkillNode node in classTree.nodes)
        {
            if (node.prerequisiteNodeIDs == null) continue;

            foreach (string prereqID in node.prerequisiteNodeIDs)
            {
                if (!nodeRectMap.ContainsKey(node.nodeID) || !nodeRectMap.ContainsKey(prereqID))
                    continue;

                // Find the prerequisite node to check if it's unlocked
                SkillNode prereqNode = classTree.nodes.Find(n => n.nodeID == prereqID);
                bool prereqUnlocked = prereqNode != null && prereqNode.isUnlocked;

                nodeConnector.DrawLine(
                    nodeRectMap[prereqID],
                    nodeRectMap[node.nodeID],
                    lineContainer,
                    prereqUnlocked // line is white if prereq is unlocked, grey if not
                );
            }
        }
    }
    
    public void AnimateNodeLines(SkillNode unlockedNode, SkillTreeData tree)
    {
        // Find which class tree this node belongs to
        SkillClassTree classTree = null;
        Transform lineContainer = null;

        if (tree.tankTree.nodes != null && tree.tankTree.nodes.Contains(unlockedNode))
        {
            classTree = tree.tankTree;
            lineContainer = tankLineContainer;
        }
        else if (tree.damageTree.nodes != null && tree.damageTree.nodes.Contains(unlockedNode))
        {
            classTree = tree.damageTree;
            lineContainer = damageLineContainer;
        }
        else if (tree.supportTree.nodes != null && tree.supportTree.nodes.Contains(unlockedNode))
        {
            classTree = tree.supportTree;
            lineContainer = supportLineContainer;
        }

        if (classTree == null || lineContainer == null) return;

        // Find all nodes that have the just-unlocked node as a prerequisite
        foreach (SkillNode node in classTree.nodes)
        {
            if (node.prerequisiteNodeIDs == null) continue;
            if (!node.prerequisiteNodeIDs.Contains(unlockedNode.nodeID)) continue;

            if (nodeRectMap.ContainsKey(unlockedNode.nodeID) && nodeRectMap.ContainsKey(node.nodeID))
            {
                nodeConnector.AnimateLine(
                    nodeRectMap[unlockedNode.nodeID],
                    nodeRectMap[node.nodeID],
                    lineContainer
                );
            }
        }
    }

    public void RefreshAllNodes()
    {
        foreach (var node in spawnedNodes)
            node.UpdateVisual();

        if (currentTree == null) return;
        StartCoroutine(DelayedLineRedraw());
    }

    private System.Collections.IEnumerator DelayedLineRedraw()
    {
        // Wait for any running wipe animations to finish
        yield return new WaitForSecondsRealtime(nodeConnector.fillDuration + 0.1f);

        if (currentTree == null) yield break;
        DrawTreeLines(currentTree.tankTree, tankLineContainer);
        DrawTreeLines(currentTree.damageTree, damageLineContainer);
        DrawTreeLines(currentTree.supportTree, supportLineContainer);
    }
}