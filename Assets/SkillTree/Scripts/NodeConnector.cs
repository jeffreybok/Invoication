using UnityEngine;
using UnityEngine.UI;

public class NodeConnector : MonoBehaviour
{
    public static NodeConnector Instance;

    [Header("References")]
    public Transform lineContainer;
    public Sprite lineSprite;

    [Header("Style")]
    public Color lockedLineColor;
    public Color unlockedLineColor;
    public float lineThickness = 4f;

    void Awake()
    {
        Instance = this;
    }

    public void DrawLines(System.Collections.Generic.List<RectTransform> nodeRects,
        System.Collections.Generic.List<SkillNode> nodes)
    {
        // Clear previous lines
        foreach (Transform child in lineContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < nodeRects.Count - 1; i++)
        {
            CreateLine(nodeRects[i], nodeRects[i + 1], nodes[i].isUnlocked);
        }
    }

    private void CreateLine(RectTransform from, RectTransform to, bool isUnlocked)
    {
        GameObject lineObj = new GameObject("Line", typeof(Image));
        lineObj.transform.SetParent(lineContainer, false);

        Image lineImage = lineObj.GetComponent<Image>();
        lineImage.sprite = lineSprite;
        lineImage.color = isUnlocked ? unlockedLineColor : lockedLineColor;

        RectTransform rt = lineObj.GetComponent<RectTransform>();

        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos = to.anchoredPosition;

        Vector2 direction = toPos - fromPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rt.sizeDelta = new Vector2(distance, lineThickness);
        rt.anchoredPosition = fromPos + direction * 0.5f;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        rt.SetAsFirstSibling(); // lines render behind nodes
    }
}
