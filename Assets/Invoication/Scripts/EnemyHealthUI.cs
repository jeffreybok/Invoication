using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    // public TextMeshProUGUI healthText;
    public Slider healthSlider;
    public Image fillImage;
    public string enemyName = "Legendary";
    
    private Enemy enemy;
    private Camera mainCamera;
    private Transform enemyTransform;
    private Transform pelvisBone; // Track the pelvis when ragdolled
    
    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        enemyTransform = enemy != null ? enemy.transform : transform.parent;
        mainCamera = Camera.main;
        
        // Find the pelvis bone (or any central ragdoll bone)
        if (enemy != null)
        {
            Rigidbody[] rbs = enemy.GetComponentsInChildren<Rigidbody>();
            if (rbs.Length > 0)
            {
                pelvisBone = rbs[0].transform; // Usually pelvis is first
            }
        }
        
        if (nameText != null)
        {
            nameText.text = enemyName;
        }
    }
    
    void LateUpdate()
    {
        if (enemy == null) return;

        // 🔥 Get accurate height of the model
        Renderer rend = enemy.GetComponentInChildren<Renderer>();
        float height = 2f; // fallback

        if (rend != null)
            height = rend.bounds.size.y;

        // 🔥 Position above head (tweak this multiplier if needed)
        Vector3 targetPosition = enemyTransform.position;
        targetPosition.y += height + 0.7f;

        transform.position = targetPosition;

        // Always face camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }

        // Health update
        float healthPercent = enemy.GetHealthPercent();
        healthSlider.value = healthPercent;

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