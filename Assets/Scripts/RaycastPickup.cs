using UnityEngine;

public class RaycastPickup : MonoBehaviour
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
                    PickupItem();
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
        
        // Check for throw input
        if (Input.GetKeyDown(KeyCode.Q) && heldItem != null)
        {
            ThrowItem();
        }
    }

    void PickupItem()
    {
        if (currentItem != null && playerHand != null)
        {
            Debug.Log("Picked up: " + currentItemName);
            
            Vector3 originalScale = currentItem.transform.localScale;
            
            // Disable Rigidbody BEFORE parenting
            Rigidbody rb = currentItem.GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // Disable collider BEFORE parenting
            Collider col = currentItem.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            // Parent to hand
            currentItem.transform.SetParent(playerHand);
            
            // Position and scale based on item type
            if (currentItemName.ToLower().Contains("potion"))
            {
                // Potion-specific positioning
                currentItem.transform.localPosition = new Vector3(0, 0.5f, 0.6f);
                currentItem.transform.localRotation = Quaternion.Euler(0, 0, 0);
                currentItem.transform.localScale = originalScale;
            }
            else
            {
                // Default positioning (staff, etc.)
                currentItem.transform.localPosition = Vector3.zero;
                currentItem.transform.localRotation = Quaternion.identity;
                currentItem.transform.localScale = originalScale;
            }
            
            heldItem = currentItem;
            currentItem = null;
        }
    }
    
    void ThrowItem()
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
        {
            rb = heldItem.AddComponent<Rigidbody>();
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        
        // Calculate throw direction toward crosshair
        Ray aimRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 throwDirection;
        
        RaycastHit hit;
        if (Physics.Raycast(aimRay, out hit, 100f))
        {
            // Aim at what crosshair is pointing at
            throwDirection = (hit.point - heldItem.transform.position).normalized;
        }
        else
        {
            // Aim forward if nothing hit
            throwDirection = Camera.main.transform.forward;
        }
        
        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        
        heldItem = null;
    }

    void OnGUI()
    {   
        if (currentItem != null && heldItem == null)
        {
            GUI.Label(
                new Rect(Screen.width/2 - 100, Screen.height/2 + 50, 200, 30), 
                "Press E to pick up " + currentItemName
            );
        }
        
        if (heldItem != null)
        {
            GUI.Label(new Rect(10, 10, 200, 30), "Press Q to throw " + heldItem.name);

            //Only check for MagicStaff when heldItem is not null
            if (heldItem.name == "MagicStaff")
            {
                GUI.Label(new Rect(10, 40, 400, 30), "PRESS V TO TOGGLE SPELL CASTING MODE. SAY 'FIREBALL'");
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