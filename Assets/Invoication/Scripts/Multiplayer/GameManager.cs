using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PurrNet;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    [Header("UI")]
    public GameObject gameOverUI;
    public Text countdownText;

    [Header("Settings")]
    public float returnToLobbyDelay = 5f;

    private bool gameEnded = false;

    [Header("Win UI")]
    public GameObject gameWonUI;
    public Text countdownTextWon;

    // =========================
    // CHECK ALL DEAD
    // =========================

    public void CheckAllPlayersDead()
    {
        if (!isServer) return;
        if (gameEnded) return;

        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p == null) continue;

            if (p.currentHealth > 0)
                return;
        }

        Debug.Log("ALL PLAYERS DEAD");

        gameEnded = true;
        StartCoroutine(GameOverCountdown());
    }

    // =========================
    // GAME OVER FLOW
    // =========================

    IEnumerator GameOverCountdown()
    {
        float timer = returnToLobbyDelay;

        ShowGameOver_ObserversRPC();

        while (timer > 0)
        {
            UpdateCountdown_ObserversRPC(Mathf.CeilToInt(timer));
            yield return new WaitForSeconds(1f);
            timer--;
        }
        StopAllCoroutines();

        // ✅ ONLY THIS
        RPC_LoadLobby();
    }

    [ObserversRpc]
    void ShowGameOver_ObserversRPC()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p == null) continue;

            if (p.youDiedUI != null)
                p.youDiedUI.SetActive(false);
        }
    }

    [ObserversRpc]
    void UpdateCountdown_ObserversRPC(int time)
    {
        if (countdownText != null)
            countdownText.text = "Returning to lobby in " + time + "...";
    }

    // =========================
    // WIN FLOW
    // =========================

    public void TriggerWin()
    {
        if (!isServer) return;
        if (gameEnded) return;

        gameEnded = true;

        RPC_ShowWinUI();

        StartCoroutine(ReturnToLobbyCountdown_Win());
    }

    [ObserversRpc]
    void RPC_ShowWinUI()
    {
        if (gameWonUI != null)
            gameWonUI.SetActive(true);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p == null) continue;

            if (p.youDiedUI != null)
                p.youDiedUI.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator ReturnToLobbyCountdown_Win()
    {
        int time = 5;

        while (time > 0)
        {
            RPC_UpdateCountdownWon(time);
            yield return new WaitForSeconds(1f);
            time--;
        }
        StopAllCoroutines();

        // 🔥 FIX: same as game over
        RPC_LoadLobby();

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("StartScreenScene");
    }

    [ObserversRpc]
    void RPC_UpdateCountdownWon(int time)
    {
        if (countdownTextWon != null)
            countdownTextWon.text = "Returning to lobby in " + time + "...";
    }

    // =========================
    // LOBBY LOAD RPC
    // =========================

    [ObserversRpc]
    void RPC_LoadLobby()
    {
        SceneManager.LoadScene("StartScreenScene");
    }
}