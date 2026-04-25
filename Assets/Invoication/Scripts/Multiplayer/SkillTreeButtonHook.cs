using UnityEngine;
using UnityEngine.UI;

public class SkillTreeButtonHook : MonoBehaviour
{
    public bool isNext;

    private SkillTreeManager manager;

    void Start()
    {
        var ui = GetComponentInParent<SkillTreeUI>();

        if (ui == null || ui.manager == null)
        {
            Debug.LogError("[SkillTreeButtonHook] SkillTreeUI or manager missing!");
            return;
        }

        manager = ui.manager;

        var button = GetComponent<Button>();

        if (isNext)
            button.onClick.AddListener(manager.NextTree);
        else
            button.onClick.AddListener(manager.PreviousTree);
    }
}