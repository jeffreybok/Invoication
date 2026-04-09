// RaycastPickup.cs
using UnityEngine;
using PurrNet;

public class RaycastPickup : NetworkBehaviour
{
    public float pickupRange = 3f;
    public Transform playerHand;
    public LayerMask pickupLayer;
    public float throwForce = 1f;

    private GameObject currentItem;
    private string currentItemName;
    public GameObject heldItem;

    void Update()
    {
        if (!isOwner) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        bool didHit = false;

        // If no pickup layer is assigned in inspector, fall back to normal raycast
        if (pickupLayer.value == 0)
            didHit = Physics.Raycast(ray, out hit, pickupRange);
        else
            didHit = Physics.Raycast(ray, out hit, pickupRange, pickupLayer);

        if (heldItem == null && didHit)
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                currentItem = hit.collider.gameObject;
                currentItemName = currentItem.name;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    NetworkIdentity itemIdentity = currentItem.GetComponent<NetworkIdentity>();
                    NetworkIdentity playerIdentity = GetComponent<NetworkIdentity>();

                    if (itemIdentity != null && playerIdentity != null)
                    {
                        PickupItem_ServerRPC(itemIdentity, playerIdentity);
                    }
                }
            }
            else
            {
                currentItem = null;
                currentItemName = "";
            }
        }
        else
        {
            currentItem = null;
            currentItemName = "";
        }

        if (Input.GetKeyDown(KeyCode.Q) && heldItem != null)
        {
            Vector3 throwDirection;

            if (Physics.Raycast(ray, out hit, 100f))
                throwDirection = (hit.point - playerHand.position).normalized;
            else
                throwDirection = cam.transform.forward;

            NetworkIdentity playerIdentity = GetComponent<NetworkIdentity>();
            if (playerIdentity != null)
            {
                ThrowItem_ServerRPC(playerIdentity, throwDirection);
            }
        }
    }

    [ServerRpc]
    void PickupItem_ServerRPC(NetworkIdentity itemIdentity, NetworkIdentity playerIdentity)
    {
        if (!isServer) return;
        if (itemIdentity == null || playerIdentity == null) return;

        GameObject item = itemIdentity.gameObject;
        GameObject player = playerIdentity.gameObject;

        if (item == null || player == null) return;

        RaycastPickup playerPickup = player.GetComponent<RaycastPickup>();
        if (playerPickup == null) return;
        if (playerPickup.playerHand == null) return;
        if (playerPickup.heldItem != null) return;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] itemColliders = item.GetComponentsInChildren<Collider>();
        for (int i = 0; i < itemColliders.Length; i++)
        {
            itemColliders[i].enabled = false;
        }

        item.transform.SetParent(playerPickup.playerHand);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        playerPickup.heldItem = item;
    }

    [ServerRpc]
    void ThrowItem_ServerRPC(NetworkIdentity playerIdentity, Vector3 throwDirection)
    {
        if (!isServer) return;
        if (playerIdentity == null) return;

        GameObject player = playerIdentity.gameObject;
        if (player == null) return;

        RaycastPickup playerPickup = player.GetComponent<RaycastPickup>();
        if (playerPickup == null) return;
        if (playerPickup.heldItem == null) return;

        GameObject item = playerPickup.heldItem;
        playerPickup.heldItem = null;

        item.transform.SetParent(null);

        Collider[] itemColliders = item.GetComponentsInChildren<Collider>();
        for (int i = 0; i < itemColliders.Length; i++)
        {
            itemColliders[i].enabled = true;
        }

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null)
            rb = item.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(throwDirection.normalized * throwForce, ForceMode.VelocityChange);
    }

    public void SetHeldItem_Server(GameObject item)
    {
        if (!isServer) return;

        heldItem = item;

        if (heldItem == null || playerHand == null) return;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] itemColliders = heldItem.GetComponentsInChildren<Collider>();
        for (int i = 0; i < itemColliders.Length; i++)
        {
            itemColliders[i].enabled = false;
        }

        heldItem.transform.SetParent(playerHand);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
    }

    void OnGUI()
    {
        if (!isOwner) return;

        if (currentItem != null && heldItem == null)
        {
            GUI.Label(
                new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30),
                "Press E to pick up " + currentItemName
            );
        }

        if (heldItem != null)
        {
            GUI.Label(new Rect(10, 10, 200, 30), "Press Q to throw " + heldItem.name);

            string heldName = heldItem.name.ToLower();
            if (heldName.Contains("magicstaff") || heldName.Contains("staff") || heldName.Contains("wand"))
            {
                GUI.Label(
                    new Rect(10, 40, 400, 30),
                    "PRESS V TO TOGGLE SPELL CASTING MODE. SAY 'FIREBALL'"
                );
            }
        }
    }

    void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * pickupRange);
        }
    }
}