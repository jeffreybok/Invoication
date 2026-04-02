using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    public Image nodeImage;

    [Header("Tint Colors")]
    public Color lockedTint = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color unlockedTint = new Color(0.7f, 0.7f, 0.7f, 1f);

    private SkillNode nodeData;
    private SkillTreeData treeData;

    public void Initialize(SkillNode node, SkillTreeData tree)
    {
        nodeData = node;
        treeData = tree;
        
        if (nodeImage != null && node.nodeIcon != null)
            nodeImage.sprite = node.nodeIcon;
        
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        var manager = FindFirstObjectByType<SkillTreeManager>();

        if (nodeData.isUnlocked)
            nodeImage.color = unlockedTint;
        else
            nodeImage.color = lockedTint;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipUI.Instance.Show(nodeData.nodeName, nodeData.nodeDescription, nodeData.skillPointCost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var manager = FindFirstObjectByType<SkillTreeManager>();
        if (manager == null || !manager.CanUnlock(treeData, nodeData))
            return;
        ConfirmationUI.Instance.Show(nodeData, treeData, this);
    }
}