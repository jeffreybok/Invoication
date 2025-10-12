using UnityEngine;

public class FireballCaster : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballSpeed = 20f;
    public KeyCode castKey = KeyCode.Mouse0;
    
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
        // Debug what we're holding
        if (Input.GetKeyDown(castKey))
        {
            Debug.Log("Cast key pressed");
            
            if (pickupScript != null && pickupScript.heldItem != null)
            {
                Debug.Log("Holding item: " + pickupScript.heldItem.name);
            }
            else
            {
                Debug.Log("Not holding anything");
            }
        }
        
        // Only cast if holding the staff
        if (IsHoldingStaff() && Input.GetKeyDown(castKey))
        {
            CastFireball();
        }
    }
    
    bool IsHoldingStaff()
    {
        if (pickupScript != null && pickupScript.heldItem != null)
        {
            string itemName = pickupScript.heldItem.name.ToLower();
            Debug.Log("Checking item name: " + itemName);
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
        
        // Fresh raycast for EACH fireball
        Ray aimRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 targetPoint;
        
        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f))
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
            // Set velocity immediately
            rb.linearVelocity = shootDirection * fireballSpeed;
            rb.useGravity = false; // Make sure gravity doesn't affect it
        }
        
        Destroy(fireball, 5f);
    }
}