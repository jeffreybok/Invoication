using UnityEngine;
using PurrNet;

public class SpellCaster : NetworkBehaviour
{
    public GameObject fireballPrefab;
    public GameObject blazingImpactPrefab;
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
            CastBlazingImpact();
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

    void LaunchProjectile_Server(GameObject prefab, float speed, Vector3 spawnPos, Vector3 direction, GameObject player)
    {
        NetworkIdentity projectileIdentity = Instantiate(prefab, spawnPos, Quaternion.identity)
            .GetComponent<NetworkIdentity>();

        Rigidbody rb = projectileIdentity.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            rb.useGravity = false;
        }

        // Ignore collision with the caster
        Collider projectileCollider = projectileIdentity.GetComponent<Collider>();
        Collider playerCollider = player.GetComponent<Collider>();

        if (projectileCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(projectileCollider, playerCollider);
        }

        Destroy(projectileIdentity.gameObject, 5f);
    }

    [ServerRpc]
    void LaunchProjectile_ServerRpc(NetworkIdentity playerIdentity, int spellType, Vector3 clientForward)
    {
        GameObject prefab = null;
        float speed = fireballSpeed;

        if (spellType == 0)
        {
            prefab = fireballPrefab;
            speed = fireballSpeed;
        }
        else if (spellType == 1)
        {
            prefab = blazingImpactPrefab;
            speed = fireballSpeed;
        }
        else if (spellType == 2)
        {
            prefab = iceballPrefab;
            speed = iceballSpeed;
        }

        if (prefab == null)
        {
            Debug.LogError("Prefab not assigned!");
            return;
        }

        GameObject player = playerIdentity.gameObject;

        Camera cam = player.GetComponentInChildren<Camera>();
        Transform spawnPoint = player.GetComponent<SpellCaster>().fireballSpawnPoint;

        Vector3 spawnPos = spawnPoint != null
            ? spawnPoint.position + clientForward * 2f
            : cam.transform.position + clientForward * 2f;

        Ray aimRay = new Ray(cam.transform.position, clientForward);
        Vector3 targetPoint;

        if (Physics.Raycast(aimRay, out RaycastHit hit, 100f, aimLayers) && hit.distance > 3f)
            targetPoint = hit.point;
        else
            targetPoint = aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;

        LaunchProjectile_Server(prefab, speed, spawnPos, shootDirection, player);
    }

    public void CastFireball()
    {
        Debug.Log("CASTING FIREBALL!");
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 0, GetComponentInChildren<Camera>().transform.forward);
    }

    public void CastBlazingImpact()
    {
        Debug.Log("CASTING BLAZING IMPACT!");
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 1, GetComponentInChildren<Camera>().transform.forward);
    }

    public void CastIceball()
    {
        Debug.Log("CASTING ICEBALL!");
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 2, GetComponentInChildren<Camera>().transform.forward);
    }
}