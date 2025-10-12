using UnityEngine;

public class StaffPickup : MonoBehaviour
{
    public GameObject wandPrefab; // The wand model that appears in player's hand
    public Transform playerHand; // Where the wand should appear (child of camera)
    
    private bool playerNearby = false;
    private GameObject player;

    void Update()
    {
        // Check if player presses E while nearby
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PickupWand();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.gameObject;
            // Optional: Show UI prompt "Press E to pick up"
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            player = null;
            // Optional: Hide UI prompt
        }
    }

    void PickupWand()
    {
        // Spawn wand in player's hand
        if (playerHand != null && wandPrefab != null)
        {
            Instantiate(wandPrefab, playerHand.position, playerHand.rotation, playerHand);
        }
        
        // Destroy the floor wand
        Destroy(gameObject);
    }
    // Add this function here, in the same script
    void OnGUI()
    {
        if (playerNearby)
        {
            GUI.Label(new Rect(Screen.width/2 - 75, Screen.height/2 + 50, 150, 30), 
                      "Press E to pick up wand");
        }
    }
}
