using UnityEngine;
using PurrNet;

public class StaffPickup : NetworkBehaviour
{
    public NetworkIdentity wandPrefab; // Networked wand prefab
    public Transform playerHand; // Where the wand appears

    private bool playerNearby = false;
    private GameObject player;

    void Update()
    {
        // if (!isOwner) return;
        if (!isServer) return; // FOR SINGLE PLAYER DEMO

        // Check if player presses E while nearby
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PickupWand_ServerRPC();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            player = null;
        }
    }

    [ServerRpc]
    void PickupWand_ServerRPC()
    {
        if (wandPrefab == null || playerHand == null) return;

        // Spawn wand in player's hand
        NetworkIdentity wand = Instantiate(
            wandPrefab,
            playerHand.position,
            playerHand.rotation
        );

        wand.transform.SetParent(playerHand);

        // Destroy floor wand
        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (!isOwner) return;

        if (playerNearby)
        {
            GUI.Label(
                new Rect(Screen.width / 2 - 75, Screen.height / 2 + 50, 150, 30),
                "Press E to pick up wand"
            );
        }
    }
}