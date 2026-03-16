using UnityEngine;

public enum NodeType { Major, Minor }

public enum NodeEffect 
{
    None,

    // Fireball
    FireballFlatDamage,
    FireballPercentDamage,
    FireballExplosionRadius,

    // Blazing Impact
    BlazingFlatDamage,
    BlazingPercentDamage,
    BlazingBurnDamage,
    BlazingBurnDuration,

    // Ice Spike
    IceSpikeFlatDamage,
    IceSpikePercentDamage,
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
    
    [Header("Effect")]
    public NodeEffect nodeEffect;
    public float effectValue;
    
    [Header("State")]
    public bool isUnlocked = false;
}
