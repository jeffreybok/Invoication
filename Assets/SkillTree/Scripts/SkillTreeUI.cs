using UnityEngine;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    [Header("References")]
    public GameObject majorNodePrefab;
    public GameObject minorNodePrefab;
    public Transform nodeContainer;
    public float nodeSpacing = 120;
    
    private SkillTreeData currentTree;
    private List<SkillNodeUI> spawnedNodes = new List<SkillNodeUI>();
    private List<RectTransform> spawnedRects = new List<RectTransform>();
    
    public void LoadTree(SkillTreeData tree)
    {
        currentTree = tree;
        
        // Clear previous nodes
        foreach (Transform child in nodeContainer)
        {
            if (child.gameObject.name != "LineContainer")
                Destroy(child.gameObject);
        }
        
        spawnedNodes.Clear();
        spawnedRects.Clear();
        
        // Spawn nodes in order
        for (int i = 0; i < tree.nodes.Count; i++)
        {
            SkillNode node = tree.nodes[i];
            GameObject prefab = node.nodeType == NodeType.Major ? majorNodePrefab : minorNodePrefab;

            GameObject spawned = Instantiate(prefab, nodeContainer);
            RectTransform rt = spawned.GetComponent<RectTransform>();
            float startX = -((tree.nodes.Count - 1) * nodeSpacing) / 2f;
            rt.anchoredPosition = new Vector2(startX + (i * nodeSpacing), 0);

            SkillNodeUI nodeUI = spawned.GetComponent<SkillNodeUI>();
            nodeUI.Initialize(node, tree, i);
            spawnedNodes.Add(nodeUI);
            spawnedRects.Add(rt);
        }
        
        NodeConnector.Instance.DrawLines(spawnedRects, tree.nodes);
    }
    
    public void RefreshAllNodes()
    {
        foreach (var node in spawnedNodes)
            node.UpdateVisual();
        
        NodeConnector.Instance.DrawLines(spawnedRects, currentTree.nodes);
    }
}
