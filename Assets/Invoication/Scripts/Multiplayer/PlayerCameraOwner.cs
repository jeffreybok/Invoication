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
        Debug.Log($"[PlayerCameraOwner] Spawned | isOwner: {isOwner} | isServer: {isServer}");

        if (!isOwner)
        {
            Debug.Log("[PlayerCameraOwner] NOT OWNER → disabling camera");

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            if (audioListener != null)
                audioListener.enabled = false;
        }
        else
        {
            Debug.Log("[PlayerCameraOwner] I OWN THIS PLAYER → enabling camera");

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);
        }
    }
}