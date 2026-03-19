using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    public float gridSize = 4f;

    [ContextMenu("Snap Children To Grid")]
    void Snap()
    {
        foreach (Transform child in transform)
        {
            Vector3 pos = child.position;

            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
            pos.z = Mathf.Round(pos.z / gridSize) * gridSize;

            child.position = pos;
        }
    }
}