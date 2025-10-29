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
    
    private RaycastPickup pickupScript;
    
    void Start()
    {
        pickupScript = GetComponent<RaycastPickup>();
        
        if (pickupScript == null)
        {
            Debug.LogError("RaycastPickup script not found!");
        }
    }
    
    void Update()
    {
        // Only cast if holding the staff and pressing the button
        if (Input.GetKeyDown(castKey) && IsHoldingStaff())
        {
            CastFireball();
        }
    }
    
    bool IsHoldingStaff()
    {
        if (pickupScript != null && pickupScript.heldItem != null)
        {
            string itemName = pickupScript.heldItem.name.ToLower();
            // REMOVED the Debug.Log from here - it was spamming!
            return itemName.Contains("staff") || itemName.Contains("wand") || itemName.Contains("magic");
        }
        return false;
    }
    
    public void CastFireball()
    {
        Debug.Log("CASTING FIREBALL!");

        if (fireballPrefab == null)
        {
            Debug.LogError("Fireball prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos;
        if (fireballSpawnPoint != null)
        {
            spawnPos = fireballSpawnPoint.position + 
                    Vector3.up * 1.2f + 
                    Camera.main.transform.forward * 0.8f;
        }
        else
        {
            spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        }
        
        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;
        
        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f, aimLayers) && hit.distance > 3f)
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = aimRay.GetPoint(100f);
        }
        
        Vector3 shootDirection = (targetPoint - spawnPos).normalized;
        
        GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        
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
        Debug.Log("CASTING ICEBALL!");

        if (iceballPrefab == null)
        {
            Debug.LogError("Iceball prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos;
        if (fireballSpawnPoint != null)
        {
            spawnPos = fireballSpawnPoint.position + 
                    Vector3.up * 1.2f + 
                    Camera.main.transform.forward * 0.8f;
        }
        else
        {
            spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        }
        
        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;
        
        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f, aimLayers) && hit.distance > 3f)
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = aimRay.GetPoint(100f);
        }
        
        Vector3 shootDirection = (targetPoint - spawnPos).normalized;
        
        GameObject iceball = Instantiate(iceballPrefab, spawnPos, Quaternion.identity);
        
        Rigidbody rb = iceball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * iceballSpeed;
            rb.useGravity = false;
        }
    }
}