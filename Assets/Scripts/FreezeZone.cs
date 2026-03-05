using UnityEngine;

public class FreezeZone : MonoBehaviour
{
    public float freezeRadius = 2f;
    public float freezeDuration = 3f;

    // 🔥 Add this
    public GameObject owner;

    void Start()
    {
        Debug.Log("Checking freeze radius at position: " + transform.position + " with radius: " + freezeRadius);
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, freezeRadius);
        
        Debug.Log("Found " + hitColliders.Length + " colliders in freeze zone");
        
        foreach (Collider hit in hitColliders)
        {
            // 🔥 Ignore the shooter (and anything under them)
            if (owner != null && hit.transform.root.gameObject == owner)
            {
                Debug.Log("Skipping owner: " + hit.gameObject.name);
                continue;
            }

            Debug.Log("Found collider: " + hit.gameObject.name + " - checking for Enemy component");
            
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsFrozen())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                Debug.Log("Freezing enemy: " + enemy.name + " at distance: " + distance);
                enemy.Freeze(freezeDuration);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, freezeRadius);
    }
}