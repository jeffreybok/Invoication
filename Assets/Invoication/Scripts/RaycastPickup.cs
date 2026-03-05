using UnityEngine;
using PurrNet;

public class RaycastPickup : NetworkBehaviour
{
    public float pickupRange = 3f;
    public Transform playerHand;
    public LayerMask pickupLayer;

    // Throw settings
    public float throwForce = 1f;

    private GameObject currentItem;
    private string currentItemName;
    public GameObject heldItem; // Track what we're holding

    void Update()
    {
        if (!isOwner) return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.green);

        // Only look for items if not already holding something
        if (heldItem == null && Physics.Raycast(ray, out hit, pickupRange))
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
            }
        }
        else
        {
            currentItem = null;
        }

        // Throw input
        if (Input.GetKeyDown(KeyCode.Q) && heldItem != null)
        {
            Ray aimRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 throwDirection;

            if (Physics.Raycast(aimRay, out hit, 100f))
                throwDirection = (hit.point - heldItem.transform.position).normalized;
            else
                throwDirection = Camera.main.transform.forward;

            ThrowItem_ServerRPC(throwDirection);
        }
    }

    [ServerRpc]
    void PickupItem_ServerRPC(NetworkIdentity itemIdentity, NetworkIdentity playerIdentity)
    {
        if (itemIdentity == null || playerIdentity == null) return;

        GameObject item = itemIdentity.gameObject;
        GameObject player = playerIdentity.gameObject;

        RaycastPickup playerPickup = player.GetComponent<RaycastPickup>();
        if (playerPickup == null) return;

        Transform hand = playerPickup.playerHand;
        if (hand == null) return;

        Debug.Log("Picked up: " + item.name);

        Vector3 originalScale = item.transform.localScale;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        item.transform.SetParent(hand);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = originalScale;

        playerPickup.heldItem = item;
    }

    [ServerRpc]
    void ThrowItem_ServerRPC(Vector3 throwDirection)
    {
        if (heldItem == null) return;

        Debug.Log("Throwing: " + heldItem.name);

        Vector3 currentScale = heldItem.transform.localScale;

        heldItem.transform.SetParent(null);
        heldItem.transform.localScale = currentScale;

        Collider col = heldItem.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();

        if (rb == null)
            rb = heldItem.AddComponent<Rigidbody>();
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);

        heldItem = null;
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

            if (heldItem.name == "MagicStaff")
            {
                GUI.Label(new Rect(10, 40, 400, 30),
                    "PRESS V TO TOGGLE SPELL CASTING MODE. SAY 'FIREBALL'");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Camera.main.transform.position,
                           Camera.main.transform.forward * pickupRange);
        }
    }
}