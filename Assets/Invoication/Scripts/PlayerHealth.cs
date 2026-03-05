using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI References")]
    public Text healthText;

    [Header("Game Over")]
    public GameObject gameOverScreen;
    public string mainMenuSceneName = "StartScreenScene";

    public bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthText != null)
            healthText.text = "HP: " + currentHealth.ToString("0") + " / " + maxHealth.ToString("0");
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");

        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToMainMenu()
    {  
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreenScene");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            Heal(10f);
        if (Input.GetKeyDown(KeyCode.J))
            TakeDamage(10f);
    }
}