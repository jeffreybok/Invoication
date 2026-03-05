using UnityEngine;
using UnityEngine.SceneManagement;

public class EndZone : MonoBehaviour
{
    public string endSceneName = "EndScreen"; // Or use scene index
    public bool useSceneIndex = false;
    public int endSceneIndex = 0;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EndGame();
        }
    }
    
    void EndGame()
    {
        Debug.Log("Game Complete!");
        
        if (useSceneIndex)
        {
            SceneManager.LoadScene(endSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(endSceneName);
        }
    }
}