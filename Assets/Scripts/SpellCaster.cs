using UnityEngine;
using PurrNet;

public class SpellCaster : NetworkBehaviour
{
    // IMPORTANT: drag the Fireball prefab's NetworkIdentity component here
    public NetworkIdentity fireballPrefab;

    // Keeping your iceball setup as-is (we can network it next)
    public GameObject iceballPrefab;

    public Transform fireballSpawnPoint;
    public float fireballSpeed = 20f;
    public float iceballSpeed = 20f;
    public KeyCode castKey = KeyCode.Mouse0;
    public LayerMask aimLayers = ~0;

    [Header("Spell Text Popup")]
    public GameObject textPopupPrefab;
    public float textFloatSpeed = 1f;
    public float textFadeDuration = 2f;

    private RaycastPickup pickupScript;

    void Start()
    {
        pickupScript = GetComponent<RaycastPickup>();

        if (pickupScript == null)
            Debug.LogError("RaycastPickup script not found!");
    }

    void Update()
    {
        if (!isOwner) return;

        if (Input.GetKeyDown(castKey) && IsHoldingStaff())
        {
            CastIceball();
        }
    }

    bool IsHoldingStaff()
    {
        if (pickupScript != null && pickupScript.heldItem != null)
        {
            string itemName = pickupScript.heldItem.name.ToLower();
            return itemName.Contains("staff") || itemName.Contains("wand") || itemName.Contains("magic");
        }
        return false;
    }

    public void CastFireball()
    {
        Debug.Log("CASTING FIREBALL!");

        if (fireballPrefab == null)
        {
            Debug.LogError("Fireball NetworkIdentity prefab not assigned!");
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 1.8f;

        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;

        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f, aimLayers) && hit.distance > 3f)
            targetPoint = hit.point;
        else
            targetPoint = aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;

        // Ask host to spawn it so everyone sees it
        CastFireball_ServerRPC(spawnPos, shootDirection);
    }

    [ServerRpc]
    void CastFireball_ServerRPC(Vector3 spawnPos, Vector3 direction)
    {
        // Instantiate automatically spawns for all clients
        NetworkIdentity fireballIdentity = Instantiate(
            fireballPrefab,
            spawnPos,
            Quaternion.LookRotation(direction)
        );

        // ✅ NO ownership — server owns it, everyone sees it
        // fireballIdentity.GiveOwnership(...) is NOT called

        GameObject fireball = fireballIdentity.gameObject;

        Fireball fb = fireball.GetComponent<Fireball>();
        if (fb != null)
            fb.SetOwner(gameObject);

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * fireballSpeed;
            rb.useGravity = false;
        }

        // Auto-despawn for everyone after 5 seconds
        Destroy(fireball, 5f);
    }

    public void CastIceball()
    {
        Debug.Log("CASTING ICEBALL!");

        if (iceballPrefab == null)
        {
            Debug.LogError("Iceball prefab not assigned!");
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 1.8f;

        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;

        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f, aimLayers) && hit.distance > 3f)
            targetPoint = hit.point;
        else
            targetPoint = aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;

        // Still local for now (we'll network this next)
        GameObject iceball = Instantiate(iceballPrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = iceball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * iceballSpeed;
            rb.useGravity = false;
        }

        Destroy(iceball, 5f);
    }
}