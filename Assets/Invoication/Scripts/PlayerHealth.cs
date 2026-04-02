using UnityEngine;
using UnityEngine.UI;
using PurrNet;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    public bool isDead = false;

    [Header("UI")]
    public Text healthText;
    public GameObject youDiedUI;

    void Start()
    {
        currentHealth = maxHealth;

        if (youDiedUI != null)
            youDiedUI.SetActive(false);

        UpdateUI();
    }

    // =========================
    // DAMAGE
    // =========================

    public void TakeDamage(float dmg)
    {
        if (!isServer) return;
        if (isDead) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI_ObserversRPC(currentHealth);

        if (currentHealth <= 0)
        {
            Die_Server();
        }
    }

    // =========================
    // HEAL
    // =========================

    public void Heal(float amount)
    {
        if (!isServer) return;
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI_ObserversRPC(currentHealth);
    }

    // =========================
    // DEATH
    // =========================

    void Die_Server()
    {
        if (isDead) return;

        isDead = true;

        StartFlicker_ObserversRPC();
        Die_ObserversRPC();

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.CheckAllPlayersDead();
    }

    // =========================
    // FLICKER
    // =========================

    [ObserversRpc]
    void StartFlicker_ObserversRPC()
    {
        var flicker = GetComponent<DeathFlicker>();
        if (flicker != null)
            flicker.StartFlicker();
    }

    // =========================
    // CLIENT SIDE
    // =========================

    [ObserversRpc]
    void Die_ObserversRPC()
    {
        if (!isOwner) return;

        Debug.Log("You died");

        if (youDiedUI != null)
            youDiedUI.SetActive(true);

        // disable movement
        var controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;
    }

    // =========================
    // UI SYNC
    // =========================

    [ObserversRpc]
    void UpdateUI_ObserversRPC(float newHealth)
    {
        currentHealth = newHealth;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = "HP: " + currentHealth.ToString("0");
    }
}