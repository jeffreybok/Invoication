using UnityEngine;
using PurrNet;

public class PlayerCameraOwner : NetworkBehaviour
{
    private Camera playerCamera;
    private AudioListener audioListener;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        audioListener = GetComponentInChildren<AudioListener>();
    }

    protected override void OnSpawned()
    {
        if (!isOwner)
        {
            // Disable camera + audio for non-owners
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            if (audioListener != null)
                audioListener.enabled = false;
        }
        else
        {
            // Ensure owner camera is enabled
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);
        }
    }
}