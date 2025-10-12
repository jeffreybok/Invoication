using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject player; // Reference to your player
    
    private bool isPaused = false;
    private PlayerController playerController;
    private RaycastPickup raycastPickup;

    void Start()
    {
        // Get references to the player scripts
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            
            // Also get the RaycastPickup script (it's on the Camera)
            raycastPickup = player.GetComponentInChildren<RaycastPickup>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Re-enable player scripts (but NOT camera components)
        if (playerController != null) playerController.enabled = true;
        if (raycastPickup != null) raycastPickup.enabled = true;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Disable player scripts (but NOT camera components)
        if (playerController != null) playerController.enabled = false;
        if (raycastPickup != null) raycastPickup.enabled = false;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Time.timeScale = 1f; // Reset time before quitting
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
