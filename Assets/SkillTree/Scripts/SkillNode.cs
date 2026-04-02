using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkillNode", menuName = "SkillTree/SkillNode")]
public class SkillNode : ScriptableObject
{
    [Header("Node Information")]
    public string nodeName;
    public string nodeID;
    public string nodeDescription;
    public int skillPointCost;
    public Sprite nodeIcon;
    
    [Header("Prerequisites")]
    public List<string> prerequisiteNodeIDs = new List<string>();
    
    [Header("State")]
    public bool isUnlocked = false;
}
