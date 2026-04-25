using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image nodeImage;

    public Color lockedTint = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color unlockedTint = new Color(0.7f, 0.7f, 0.7f, 1f);

    private SkillNode nodeData;
    private SkillTreeData treeData;

    private SkillTreeManager manager;
    private TooltipUI tooltip;
    private ConfirmationUI confirmation;

    public void Initialize(SkillNode node, SkillTreeData tree, SkillTreeManager mgr)
    {
        nodeData = node;
        treeData = tree;
        manager = mgr;

        tooltip = manager.tooltipUI;
        confirmation = manager.confirmationUI;

        if (nodeImage != null && node.nodeIcon != null)
            nodeImage.sprite = node.nodeIcon;

        UpdateVisual();
    }

    public void UpdateVisual()
    {
        nodeImage.color = nodeData.isUnlocked ? unlockedTint : lockedTint;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.Show(nodeData.nodeName, nodeData.nodeDescription, nodeData.skillPointCost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!manager.CanUnlock(treeData, nodeData))
            return;

        confirmation.Show(nodeData, treeData, this);
    }
}