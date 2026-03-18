using UnityEngine;

public class SnapWallsToGrid : MonoBehaviour
{
    public float gridSize = 4f;
    public float baseY = 0f;

    [ContextMenu("Snap Walls To Grid")]
    void Snap()
    {
        foreach (Transform child in transform)
        {
            Vector3 pos = child.position;

            // Snap horizontal (layout)
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.z = Mathf.Round(pos.z / gridSize) * gridSize;

            // Snap vertical (stacking)
            float height = pos.y - baseY;
            height = Mathf.Round(height / gridSize) * gridSize;
            pos.y = baseY + height;

            child.position = pos;

            // OPTIONAL: lock rotation to clean 90° angles
            Vector3 rot = child.eulerAngles;
            rot.y = Mathf.Round(rot.y / 90f) * 90f;
            child.eulerAngles = rot;
        }
    }
}