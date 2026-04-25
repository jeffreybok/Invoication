using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmationUI : MonoBehaviour
{
    public GameObject confirmationPanel;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;

    private SkillNode pendingNode;
    private SkillTreeData pendingTree;
    private SkillNodeUI pendingCaller;
    private SkillTreeManager manager;

    public void Initialize(SkillTreeManager mgr)
    {
        manager = mgr;
    }

    void Awake()
    {
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
        // 🔥 PLAY PURCHASE SOUND FIRST
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayPurchase();

        manager.UnlockNode(pendingTree, pendingNode);
        manager.RefreshSkillPointsDisplay();

        pendingCaller.UpdateVisual();

        var treeUI = manager.skillTreeUI;
        treeUI.RefreshAllNodes();
        treeUI.AnimateNodeLines(pendingNode, pendingTree);

        confirmationPanel.SetActive(false);
    }

    void OnCancel()
    {
        confirmationPanel.SetActive(false);
    }
}