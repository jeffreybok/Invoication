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
    private int nodeIndex;
    private IPointerClickHandler pointerClickHandlerImplementation;

    public void Initialize(SkillNode node, SkillTreeData tree, int index)
    {
        nodeData = node;
        treeData = tree;
        nodeIndex = index;
        
        nodeNameText.text = node.nodeName;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (nodeData.isUnlocked)
            nodeImage.color = unlockedColor;
        else if (SkillTreeManager.Instance.CanUnlock(treeData, nodeIndex))
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
        if (!SkillTreeManager.Instance.CanUnlock(treeData, nodeIndex))
            return;
        ConfirmationUI.Instance.Show(nodeData, treeData, nodeIndex, this);
    }
}
