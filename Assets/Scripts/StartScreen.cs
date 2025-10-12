using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    public void PlayGame()
    {
        // Load your main game scene
        SceneManager.LoadScene("TutorialLevel"); // Replace with your game scene name
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}