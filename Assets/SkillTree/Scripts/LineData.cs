using UnityEngine;

public class LineData : MonoBehaviour
{
    public Vector2 fromPos;
    public Vector2 toPos;
    public float fullDistance;
    public Vector2 direction;

    public void Setup(Vector2 from, Vector2 to, float distance)
    {
        fromPos = from;
        toPos = to;
        fullDistance = distance;
        direction = (to - from).normalized;
    }
}