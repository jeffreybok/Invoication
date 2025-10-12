using UnityEngine;

public class PotionShatter : MonoBehaviour
{
    public GameObject freezeCirclePrefab;
    public float shatterVelocity = 2f;
    public float effectDuration = 3f;
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > shatterVelocity)
        {
            Shatter(collision.contacts[0].point);
        }
    }
    
    void Shatter(Vector3 impactPoint)
    {
        if (freezeCirclePrefab != null)
        {
            // Raycast down to find ground
            Vector3 spawnPos = impactPoint;
            
            if (Physics.Raycast(impactPoint, Vector3.down, out RaycastHit hit, 100f))
            {
                spawnPos = hit.point;
            }
            
            GameObject effect = Instantiate(freezeCirclePrefab, spawnPos, Quaternion.identity);

            // FreezeZone is already on the prefab, just set duration
            FreezeZone freezeZone = effect.GetComponent<FreezeZone>();
            if (freezeZone != null)
            {
                freezeZone.freezeDuration = effectDuration;
            }

            Destroy(effect, effectDuration);
        }
        
        Destroy(gameObject);
    }
}