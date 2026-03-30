using UnityEngine;
using PurrNet;

public class PlayerUIOwner : NetworkBehaviour
{
    [SerializeField] public GameObject playerUI; 

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