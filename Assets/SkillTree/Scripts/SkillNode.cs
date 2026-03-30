using UnityEngine;
using System.Collections.Generic;

public enum NodeType { Major, Minor }

public enum NodeEffect 
{
    None,

    // Fireball
    FireballFlatDamage,
    FireballExplosionRadius,

    // Blazing Impact
    BlazingFlatDamage,
    BlazingBurnDamage,
    BlazingBurnDuration,

    // Ice Spike
    IceSpikeFlatDamage,
    IceSpikeSlowDuration,
    IceSpikeFreezeRadius
}

[CreateAssetMenu(fileName = "NewSkillNode", menuName = "SkillTree/SkillNode")]
public class SkillNode : ScriptableObject
{
    [Header("Node Information")]
    public string nodeName;
    public string nodeID;
    public string nodeDescription;
    public int skillPointCost;
    public NodeType nodeType;
    
    [Header("Prerequisites")]
    public List<string> prerequisiteNodeIDs = new List<string>();
    
    [Header("Effect")]
    public NodeEffect nodeEffect;
    public float effectValue;
    
    [Header("State")]
    public bool isUnlocked = false;
}
