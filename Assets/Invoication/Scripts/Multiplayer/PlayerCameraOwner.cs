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

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            if (audioListener != null)
                audioListener.enabled = false;
        }
        else
        {

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);
        }
    }
}