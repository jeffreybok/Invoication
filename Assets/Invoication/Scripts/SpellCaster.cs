using UnityEngine;

public class SpellCaster : MonoBehaviour
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

    // Shared helper — all projectile spells use this
    void LaunchProjectile(GameObject prefab, float speed)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab not assigned!");
            return;
        }

        Vector3 spawnPos = fireballSpawnPoint != null
            ? fireballSpawnPoint.position + Vector3.up * 1.2f + Camera.main.transform.forward * 0.8f
            : Camera.main.transform.position + Camera.main.transform.forward * 0.5f;

        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(aimRay, out RaycastHit hit, 100f, aimLayers) && hit.distance > 3f)
            targetPoint = hit.point;
        else
            targetPoint = aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;

        GameObject projectile = Instantiate(prefab, spawnPos, Quaternion.identity);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * speed;
            rb.useGravity = false;
        }

        Destroy(projectile, 5f);
    }

    public void CastFireball()
    {
        Debug.Log("CASTING FIREBALL!");
        LaunchProjectile(fireballPrefab, fireballSpeed);
    }

    public void CastBlazingImpact()
    {
        Debug.Log("CASTING BLAZING IMPACT!");
        LaunchProjectile(blazingImpactPrefab, fireballSpeed);
    }

    public void CastIceball()
    {
        Debug.Log("CASTING ICEBALL!");
        LaunchProjectile(iceballPrefab, iceballSpeed);
    }
}