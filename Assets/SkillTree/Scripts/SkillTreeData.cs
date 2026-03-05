using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkillTree", menuName = "SkillTree/SkillTreeData")]
public class SkillTreeData : ScriptableObject
{
    [Header("Element Info")]
    public string elementName;
    public Sprite elementIcon;
    
    [Header("Nodes Order")]
    public List<SkillNode> nodes;
}
