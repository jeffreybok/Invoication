using UnityEngine;

public static class SkillTreeSaveSystem
{
    private const string skillPointsKey = "skillPoints";
    private const string XPKey = "playerXP";
    private const string LevelKey = "playerLevel";

    public static void SaveAll(SkillTreeData[] allTrees, PlayerStats stats, PlayerXP xp)
    {
        PlayerPrefs.SetInt(skillPointsKey, stats.skillPoints);

        if (xp != null)
        {
            PlayerPrefs.SetInt(XPKey, xp.currentXP);
            PlayerPrefs.SetInt(LevelKey, xp.currentLevel);
        }

        foreach (SkillTreeData tree in allTrees)
        foreach (SkillNode node in tree.GetAllNodes())
            PlayerPrefs.SetInt(node.nodeID, node.isUnlocked ? 1 : 0);

        PlayerPrefs.Save();
    }

    public static void LoadAll(SkillTreeData[] allTrees, PlayerStats stats, PlayerXP xp)
    {
        stats.skillPoints = PlayerPrefs.GetInt(skillPointsKey, 0);

        if (xp != null)
        {
            xp.currentXP = PlayerPrefs.GetInt(XPKey, 0);
            xp.currentLevel = PlayerPrefs.GetInt(LevelKey, 1);
        }

        foreach (SkillTreeData tree in allTrees)
        foreach (SkillNode node in tree.GetAllNodes())
            node.isUnlocked = PlayerPrefs.GetInt(node.nodeID, 0) == 1;
    }

    public static void ResetAll(SkillTreeData[] allTrees, int defaultSkillPoints, PlayerStats stats, PlayerXP xp)
    {
        stats.skillPoints = defaultSkillPoints;

        if (xp != null)
        {
            xp.currentXP = 0;
            xp.currentLevel = 1;
            PlayerPrefs.SetInt(XPKey, 0);
            PlayerPrefs.SetInt(LevelKey, 1);
        }

        foreach (SkillTreeData tree in allTrees)
        foreach (SkillNode node in tree.GetAllNodes())
        {
            node.isUnlocked = false;
            PlayerPrefs.SetInt(node.nodeID, 0);
        }

        PlayerPrefs.SetInt(skillPointsKey, defaultSkillPoints);
        PlayerPrefs.Save();
    }
}