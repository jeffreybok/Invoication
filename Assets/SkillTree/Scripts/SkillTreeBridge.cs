using UnityEngine;

public static class SkillTreeBridge
{
    public static bool IsUnlocked(string nodeID)
    {
        return PlayerPrefs.GetInt(nodeID, 0) == 1;
    }
}
