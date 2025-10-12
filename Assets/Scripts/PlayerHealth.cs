using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Image healthBarFill;
    public Text healthText; // Optional: to display numbers

    [Header("Color Settings")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float fillAmount = currentHealth / maxHealth;
            healthBarFill.fillAmount = fillAmount;

            // Change color based on health percentage
            if (fillAmount > 0.5f)
            {
                healthBarFill.color = Color.Lerp(halfHealthColor, fullHealthColor, (fillAmount - 0.5f) * 2);
            }
            else
            {
                healthBarFill.color = Color.Lerp(lowHealthColor, halfHealthColor, fillAmount * 2);
            }
        }

        // Optional: Update text display
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString("0") + " / " + maxHealth.ToString("0");
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Add your death logic here
    }

    // Example: Test with keyboard
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(10f);
        }
    }
}
