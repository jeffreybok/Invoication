using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NodeConnector : MonoBehaviour
{
    public static NodeConnector Instance;

    [Header("Style")]
    public Color lockedLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color unlockedLineColor = Color.white;
    public float lineThickness = 4f;

    [Header("Animation")]
    public float fillDuration = 0.5f;

    void Awake() { Instance = this; }

    public void DrawLine(RectTransform from, RectTransform to, Transform lineContainer, bool isUnlocked)
    {
        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos = to.anchoredPosition;
        Vector2 direction = toPos - fromPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector2 centerPos = fromPos + direction * 0.5f;

        // Bottom layer - always grey, full width, centered
        CreateLineImage("Line_Locked", lineContainer, lockedLineColor,
            distance, lineThickness, centerPos, angle, false);

        // Top layer - white, starts at fromPos, zero width if locked
        CreateLineImage("Line_Unlocked", lineContainer, unlockedLineColor,
            isUnlocked ? distance : 0f, lineThickness,
            isUnlocked ? centerPos : fromPos,
            angle, true, fromPos, toPos, distance);
    }

    public void AnimateLine(RectTransform from, RectTransform to, Transform lineContainer)
    {
        Vector2 expectedFrom = from.anchoredPosition;

        foreach (Transform child in lineContainer)
        {
            LineData data = child.GetComponent<LineData>();
            if (data != null && Vector2.Distance(data.fromPos, expectedFrom) < 5f)
            {
                RawImage img = child.GetComponent<RawImage>();
                if (img != null)
                    StartCoroutine(WipeLine(img, child.GetComponent<RectTransform>(),
                        expectedFrom, data.fullDistance, data.direction));
                return;
            }
        }
    }

    private IEnumerator WipeLine(RawImage lineImage, RectTransform rt,
        Vector2 fromPos, float fullDistance, Vector2 direction)
    {
        float timer = 0f;
        while (timer < fillDuration)
        {
            if (rt == null || lineImage == null) yield break;

            float t = timer / fillDuration;
            float currentWidth = Mathf.Lerp(0f, fullDistance, t);

            rt.sizeDelta = new Vector2(currentWidth, lineThickness);
            rt.anchoredPosition = fromPos + direction * (currentWidth * 0.5f);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (rt == null) yield break;

        rt.sizeDelta = new Vector2(fullDistance, lineThickness);
        rt.anchoredPosition = fromPos + direction * (fullDistance * 0.5f);
    }

    private GameObject CreateLineImage(string name, Transform parent, Color color,
        float width, float thickness, Vector2 position, float angle, bool isTop,
        Vector2 fromPos = default, Vector2 toPos = default, float fullDistance = 0f)
    {
        GameObject lineObj = new GameObject(name, typeof(RawImage));
        lineObj.transform.SetParent(parent, false);

        RawImage img = lineObj.GetComponent<RawImage>();
        img.color = color;

        RectTransform rt = lineObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, thickness);
        rt.anchoredPosition = position;
        rt.localRotation = Quaternion.Euler(0, 0, angle);

        if (isTop)
        {
            lineObj.transform.SetAsLastSibling();
            LineData data = lineObj.AddComponent<LineData>();
            data.Setup(fromPos, toPos, fullDistance);
        }
        else
        {
            lineObj.transform.SetAsFirstSibling();
        }

        return lineObj;
    }
}