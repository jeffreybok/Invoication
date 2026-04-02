using UnityEngine;

public class SkillTreeButtonHook : MonoBehaviour
{
    public bool isNext;

    void Start()
    {
        var manager = FindFirstObjectByType<SkillTreeManager>();

        if (manager == null) return;

        var button = GetComponent<UnityEngine.UI.Button>();

        if (isNext)
            button.onClick.AddListener(manager.NextTree);
        else
            button.onClick.AddListener(manager.PreviousTree);
    }
}