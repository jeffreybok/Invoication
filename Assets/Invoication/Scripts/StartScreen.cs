using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    public GameObject SettingsScreen;

    private void Start()
    {
        SettingsScreen.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1_JEFFREYTEST");
    }

    public void Options()
    {
        SettingsScreen.SetActive(true);
    }

    public void BackButton()
    {
        SettingsScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }
}