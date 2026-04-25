using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;

public class PlayerHealthWorldUI : NetworkBehaviour
{
    public TextMeshProUGUI nameText;
    public Slider healthSlider;
    public Image fillImage;

    public string playerName = "Player";

    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private Camera mainCamera;

    private float cachedHeight = 2f; // 🔥 LOCKED height

    void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();
        playerTransform = playerHealth != null ? playerHealth.transform : transform.parent;
        mainCamera = Camera.main;

        if (nameText != null)
            nameText.text = playerName;

        // 🔥 CACHE HEIGHT ONCE (prevents animation shrinking issue)
        if (playerHealth != null)
        {
            Renderer rend = playerHealth.GetComponentInChildren<Renderer>();
            if (rend != null)
                cachedHeight = rend.bounds.size.y;
        }

        if (isOwner)
        {
            gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (playerHealth == null) return;

        // 🔥 USE CACHED HEIGHT (NOT recalculated)
        Vector3 targetPosition = playerTransform.position;
        targetPosition.y += cachedHeight + 4.1f;

        transform.position = targetPosition;

        // Face camera
        if (mainCamera != null)
            transform.LookAt(transform.position + mainCamera.transform.forward);

        // Health sync (already correct via ObserversRPC)
        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
        healthSlider.value = healthPercent;

        // Color
        if (fillImage != null)
        {
            if (healthPercent > 0.6f)
                fillImage.color = Color.green;
            else if (healthPercent > 0.3f)
                fillImage.color = Color.yellow;
            else
                fillImage.color = Color.red;

            if (healthPercent <= 0)
                fillImage.gameObject.SetActive(false);
        }
    }
}