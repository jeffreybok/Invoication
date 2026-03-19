using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class WallBuilderTool
{
    static bool enabled = false;
    static GameObject wallPrefab;
    static float gridSize = 4f;
    static Vector3 startPoint;
    static bool isDragging = false;

    static WallBuilderTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Wall Builder/Toggle Tool")]
    static void ToggleTool()
    {
        enabled = !enabled;
        Debug.Log("Wall Builder Tool: " + (enabled ? "ON" : "OFF"));
    }

    [MenuItem("Tools/Wall Builder/Set Wall Prefab")]
    static void SetPrefab()
    {
        wallPrefab = Selection.activeGameObject;
        Debug.Log("Wall prefab set to: " + wallPrefab.name);
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        if (!enabled || wallPrefab == null) return;

        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 snapped = Snap(hit.point);

            Handles.color = Color.green;
            Handles.DrawWireCube(snapped, Vector3.one * gridSize);

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                startPoint = snapped;
                isDragging = true;
                e.Use();
            }

            if (e.type == EventType.MouseUp && e.button == 0 && isDragging)
            {
                Vector3 endPoint = snapped;
                BuildWall(startPoint, endPoint);
                isDragging = false;
                e.Use();
            }
        }
    }

    static Vector3 Snap(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
        pos.y = 0f;
        return pos;
    }

    static void BuildWall(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            dir = new Vector3(Mathf.Sign(dir.x), 0, 0);
        else
            dir = new Vector3(0, 0, Mathf.Sign(dir.z));

        int length = Mathf.RoundToInt(Vector3.Distance(start, end) / gridSize);

        for (int i = 0; i <= length; i++)
        {
            Vector3 pos = start + dir * (i * gridSize);

            GameObject wall = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
            wall.transform.position = pos;

            if (dir.x != 0)
                wall.transform.rotation = Quaternion.Euler(0, 90, 0);

            Undo.RegisterCreatedObjectUndo(wall, "Build Wall");
        }
    }
}