using UnityEngine;

public class Iceball : MonoBehaviour
{
    [Header("Freeze Settings")]
    public float freezeRadius = 2f;
    public float freezeDuration = 3f;
    public GameObject freezeEffect;

    private GameObject owner;

    // Call this after spawning
    public void SetOwner(GameObject shooter)
    {
        owner = shooter;

        Collider myCollider = GetComponent<Collider>();
        Collider[] ownerColliders = shooter.GetComponentsInChildren<Collider>();

        // Ignore ALL colliders on the shooter
        foreach (Collider col in ownerColliders)
        {
            Physics.IgnoreCollision(myCollider, col);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Do not freeze if we somehow hit our owner
        if (owner != null && collision.transform.root.gameObject == owner)
            return;

        Freeze();
    }

    void Freeze()
    {
        Debug.Log("Iceball shattered at: " + transform.position);

        if (freezeEffect != null)
        {
            GameObject effect = Instantiate(freezeEffect, transform.position, Quaternion.identity);

            FreezeZone freezeZone = effect.AddComponent<FreezeZone>();
            freezeZone.freezeRadius = freezeRadius;
            freezeZone.freezeDuration = freezeDuration;
            freezeZone.owner = owner; // pass owner so it doesn't freeze shooter

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 2f;

            Destroy(effect, duration);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, freezeRadius);
    }
}