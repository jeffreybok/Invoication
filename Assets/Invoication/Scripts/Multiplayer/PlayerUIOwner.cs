using UnityEngine;
using PurrNet;

public class PlayerUIOwner : NetworkBehaviour
{
    [SerializeField] private GameObject playerUI; 
    // Drag your UI root here (Canvas, Crosshair, etc.)

    protected override void OnSpawned()
    {
        if (!isOwner)
        {
            if (playerUI != null)
                playerUI.SetActive(false);
        }
        else
        {
            if (playerUI != null)
                playerUI.SetActive(true);
        }
    }
}