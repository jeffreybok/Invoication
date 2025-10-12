using UnityEngine;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;
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
        // Follow ragdoll pelvis position if it exists, otherwise follow main transform
        Vector3 targetPosition;
        
        if (pelvisBone != null)
        {
            targetPosition = pelvisBone.position;
        }
        else
        {
            targetPosition = enemyTransform.position;
        }
        
        targetPosition.y += 2.5f;
        transform.position = targetPosition;
        
        // Always face camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
        
        // Update health display
        if (enemy != null && healthText != null)
        {
            healthText.text = Mathf.CeilToInt(enemy.currentHealth) + " / " + (int)enemy.maxHealth;
        }
    }
}