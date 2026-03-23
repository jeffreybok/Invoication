using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SkillClassTree
{
    public string className;
    public Sprite classIcon;
    public List<SkillNode> nodes;
}

[CreateAssetMenu(fileName = "NewSkillTree", menuName = "SkillTree/SkillTreeData")]
public class SkillTreeData : ScriptableObject
{
    [Header("Element Info")]
    public string elementName;
    public Sprite elementIcon;

    [Header("Class Trees")]
    public SkillClassTree tankTree;
    public SkillClassTree damageTree;
    public SkillClassTree supportTree;
    
    // Helper to get all nodes across all three classes
    public List<SkillNode> GetAllNodes()
    {
        List<SkillNode> all = new List<SkillNode>();
        if (tankTree.nodes != null) all.AddRange(tankTree.nodes);
        if (damageTree.nodes != null) all.AddRange(damageTree.nodes);
        if (supportTree.nodes != null) all.AddRange(supportTree.nodes);
        return all;
    }
}
