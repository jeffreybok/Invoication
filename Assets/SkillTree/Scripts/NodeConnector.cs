using UnityEngine;
using UnityEngine.UI;

public class NodeConnector : MonoBehaviour
{
    public static NodeConnector Instance;

    [Header("Style")]
    public Sprite lineSprite;
    public Color lockedLineColor;
    public Color unlockedLineColor;
    public float lineThickness = 4f;

    void Awake() { Instance = this; }

    public void DrawLine(RectTransform from, RectTransform to, Transform lineContainer, bool isUnlocked)
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
        rt.SetAsFirstSibling();
    }
}