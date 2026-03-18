using UnityEngine;

public static class SkillTreeSaveSystem
{
    private const string skillPointsKey = "skillPoints";
    private const string XPKey = "playerXP";
    private const string LevelKey = "playerLevel";

    public static void SaveAll(SkillTreeData[] allTrees, int skillPoints)
    {
        PlayerPrefs.SetInt(skillPointsKey, skillPoints);

        if (PlayerXP.Instance != null)
        {
            PlayerPrefs.SetInt(XPKey, PlayerXP.Instance.currentXP);
            PlayerPrefs.SetInt(LevelKey, PlayerXP.Instance.currentLevel);
        }
        
        foreach (SkillTreeData tree in allTrees)
        {
            foreach (SkillNode node in tree.nodes)
            {
                PlayerPrefs.SetInt(node.nodeID, node.isUnlocked ? 1 : 0);
                Debug.Log($"Saved {node.nodeID}: {node.isUnlocked}");
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log("Player Stats Saved.");
    }

    public static void LoadAll(SkillTreeData[] allTrees, out int skillPoints)
    {
        skillPoints = PlayerPrefs.GetInt(skillPointsKey, 10);

        if (PlayerXP.Instance != null)
        {
            PlayerXP.Instance.currentXP = PlayerPrefs.GetInt(XPKey, 0);
            PlayerXP.Instance.currentLevel = PlayerPrefs.GetInt(LevelKey, 1);
        }
        
        foreach (SkillTreeData tree in allTrees)
        {
            foreach (SkillNode node in tree.nodes)
            {
                int saved = PlayerPrefs.GetInt(node.nodeID, 0);
                node.isUnlocked = saved == 1;
                Debug.Log($"Loaded {node.nodeID}: isUnlocked =  {node.isUnlocked}");
            }
        }
        
        Debug.Log("Stats Loaded.");
    }

    public static void ResetAll(SkillTreeData[] allTrees, int defaultSkillPoints, out int skillPoints)
    {
        skillPoints = defaultSkillPoints;

        foreach (SkillTreeData tree in allTrees)
        {
            foreach (SkillNode node in tree.nodes)
            {
                node.isUnlocked = false;
                PlayerPrefs.SetInt(node.nodeID, 0);
            }
        }
        
        PlayerPrefs.SetInt(skillPointsKey, defaultSkillPoints);
        PlayerPrefs.Save();
        Debug.Log("Skill tree reset.");
    }
}
