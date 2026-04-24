using UnityEngine;

public class CameraHidePlayersOnDeath : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    public void HidePlayers()
    {
        int layer = LayerMask.NameToLayer("DeadHidden");
        cam.cullingMask &= ~(1 << layer);
    }
}