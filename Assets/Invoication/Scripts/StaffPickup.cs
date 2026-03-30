// StaffPickup.cs
using UnityEngine;
using PurrNet;

public class StaffPickup : NetworkBehaviour
{
    public NetworkIdentity wandPrefab;
    public Transform playerHand;

    private bool playerNearby = false;
    private GameObject player;

    void Update()
    {
        // Do NOT use isOwner on the world pickup object
        if (!playerNearby) return;
        if (player == null) return;

        NetworkIdentity playerIdentity = player.GetComponent<NetworkIdentity>();
        if (playerIdentity == null) return;
        if (!playerIdentity.isOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            PickupWand_ServerRPC(playerIdentity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent<NetworkIdentity>(out NetworkIdentity otherIdentity)) return;
        if (!otherIdentity.isOwner) return;

        playerNearby = true;
        player = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent<NetworkIdentity>(out NetworkIdentity otherIdentity)) return;
        if (!otherIdentity.isOwner) return;

        playerNearby = false;
        player = null;
    }

    [ServerRpc]
    void PickupWand_ServerRPC(NetworkIdentity playerIdentity)
    {
        if (!isServer) return;
        if (wandPrefab == null || playerIdentity == null) return;

        GameObject targetPlayer = playerIdentity.gameObject;
        if (targetPlayer == null) return;

        Transform hand = null;

        RaycastPickup raycastPickup = targetPlayer.GetComponent<RaycastPickup>();
        if (raycastPickup != null && raycastPickup.playerHand != null)
        {
            hand = raycastPickup.playerHand;
        }
        else
        {
            SpellCaster spellCaster = targetPlayer.GetComponent<SpellCaster>();
            if (spellCaster != null && spellCaster.fireballSpawnPoint != null)
                hand = spellCaster.fireballSpawnPoint;
        }

        if (hand == null)
        {
            if (playerHand != null)
                hand = playerHand;
            else
                return;
        }

        NetworkIdentity wand = Instantiate(
            wandPrefab,
            hand.position,
            hand.rotation
        );

        wand.transform.SetParent(hand);
        wand.transform.localPosition = Vector3.zero;
        wand.transform.localRotation = Quaternion.identity;

        RaycastPickup targetPickup = targetPlayer.GetComponent<RaycastPickup>();
        if (targetPickup != null)
        {
            targetPickup.SetHeldItem_Server(wand.gameObject);
        }

        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (!playerNearby) return;
        if (player == null) return;

        NetworkIdentity playerIdentity = player.GetComponent<NetworkIdentity>();
        if (playerIdentity == null) return;
        if (!playerIdentity.isOwner) return;

        GUI.Label(
            new Rect(Screen.width / 2 - 75, Screen.height / 2 + 50, 150, 30),
            "Press E to pick up wand"
        );
    }
}