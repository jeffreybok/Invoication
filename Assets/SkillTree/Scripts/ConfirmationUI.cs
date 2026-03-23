using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmationUI : MonoBehaviour
{
    public static ConfirmationUI Instance;

    [Header("References")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;

    private SkillNode pendingNode;
    private SkillTreeData pendingTree;
    private SkillNodeUI pendingCaller;

    void Awake()
    {
        Instance = this;
        confirmationPanel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    public void Show(SkillNode node, SkillTreeData tree, SkillNodeUI caller)
    {
        pendingNode = node;
        pendingTree = tree;
        pendingCaller = caller;
        messageText.text = $"Unlock {node.nodeName} for {node.skillPointCost} SP?";
        confirmationPanel.SetActive(true);
    }

    void OnConfirm()
    {
        SkillTreeManager.Instance.UnlockNode(pendingTree, pendingNode);
        pendingCaller.UpdateVisual();
        FindObjectOfType<SkillTreeUI>().RefreshAllNodes();
        SkillTreeManager.Instance.RefreshSkillPointsDisplay();
        confirmationPanel.SetActive(false);
    }

    void OnCancel()
    {
        confirmationPanel.SetActive(false);
    }
}