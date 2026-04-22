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
    public Slider healthSlider;
    public Image healthFill;
    public GameObject youDiedUI;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (youDiedUI != null)
            youDiedUI.SetActive(false);

        UpdateUI();
    }

    // =========================
    // DAMAGE (SERVER ONLY)
    // =========================
// ADD THIS INSIDE TakeDamage()

    public void TakeDamage(float dmg)
    {
        if (!isServer) return;
        if (isDead) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 🔊 PLAY DAMAGE SOUND (SERVER ONLY → NO DUPES)
        SoundManager.Instance.PlayDamage(transform.position);

        var anim = GetComponent<PlayerAnimationController>();
        if (anim != null)
            anim.PlayHit();

        UpdateUI_ObserversRPC(currentHealth);

        if (currentHealth <= 0)
        {
            Die_Server();
        }
    }

    // =========================
    // HEAL (SERVER ONLY)
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
    // DEATH (SERVER)
    // =========================
    void Die_Server()
    {
        if (isDead) return;

        isDead = true;

        var anim = GetComponent<PlayerAnimationController>();
        if (anim != null)
            anim.PlayDeath();

        StartFlicker_ObserversRPC();
        Die_ObserversRPC();

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.CheckAllPlayersDead();
    }

    // =========================
    // FLICKER (ALL CLIENTS)
    // =========================
    [ObserversRpc]
    void StartFlicker_ObserversRPC()
    {
        var flicker = GetComponent<DeathFlicker>();
        if (flicker != null)
            flicker.StartFlicker();
    }

    // =========================
    // CLIENT DEATH (OWNER ONLY)
    // =========================
    [ObserversRpc]
    void Die_ObserversRPC()
    {
        if (!isOwner) return;

        Debug.Log("You died");

        if (youDiedUI != null)
            youDiedUI.SetActive(true);

        var controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;
    }

    // =========================
    // UI SYNC (ALL CLIENTS)
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
            healthText.text = "HP: " + currentHealth.ToString("0") + " / " + maxHealth.ToString("0");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthFill != null)
            healthFill.color = Color.red;
    }
}