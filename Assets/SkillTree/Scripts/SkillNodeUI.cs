using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    public Image nodeImage;
    public TextMeshProUGUI nodeNameText;

    [Header("Colors")]
    public Color lockedColor;
    public Color unlockedColor;
    public Color availableColor;

    private SkillNode nodeData;
    private SkillTreeData treeData;

    public void Initialize(SkillNode node, SkillTreeData tree)
    {
        nodeData = node;
        treeData = tree;
        nodeNameText.text = node.nodeName;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        var manager = FindFirstObjectByType<SkillTreeManager>();

        if (nodeData.isUnlocked)
            nodeImage.color = unlockedColor;
        else if (manager != null && manager.CanUnlock(treeData, nodeData))
            nodeImage.color = availableColor;
        else
            nodeImage.color = lockedColor;
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