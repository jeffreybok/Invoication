using UnityEngine;
using System.Collections.Generic;

public enum PlayerClass { Tank, Damage, Support }

[CreateAssetMenu(fileName = "NewSkillTree", menuName = "SkillTree/SkillTreeData")]
public class SkillTreeData : ScriptableObject
{
    [Header("Element Info")]
    public string elementName;
    public Sprite elementIcon;
    public PlayerClass playerClass;
    
    [Header("Nodes Order")]
    public List<SkillNode> nodes;
}
