using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    public GameObject fireballPrefab;
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
        if (fireballPrefab == null)
        {
            Debug.LogError("Fireball prefab not assigned!");
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position +
                           Camera.main.transform.forward * 1.8f; // Spawn further forward

        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;

        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f, aimLayers) && hit.distance > 3f)
            targetPoint = hit.point;
        else
            targetPoint = aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;

        GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

        // Assign owner
        Fireball fb = fireball.GetComponent<Fireball>();
        if (fb != null)
            fb.SetOwner(gameObject);

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * fireballSpeed;
            rb.useGravity = false;
        }

        Destroy(fireball, 5f);
    }

    public void CastIceball()
    {
        if (iceballPrefab == null)
        {
            Debug.LogError("Iceball prefab not assigned!");
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position +
                           Camera.main.transform.forward * 1.8f; // Spawn further forward

        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;

        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f, aimLayers) && hit.distance > 3f)
            targetPoint = hit.point;
        else
            targetPoint = aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;

        GameObject iceball = Instantiate(iceballPrefab, spawnPos, Quaternion.identity);

        Fireball fb = iceball.GetComponent<Fireball>();
        if (fb != null)
            fb.SetOwner(gameObject);

        Rigidbody rb = iceball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * iceballSpeed;
            rb.useGravity = false;
        }

        Destroy(iceball, 5f);
    }
}