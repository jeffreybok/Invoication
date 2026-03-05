using UnityEngine;

public class Iceball : MonoBehaviour
{
    [Header("Freeze Settings")]
    public float freezeRadius = 2f;
    public float freezeDuration = 3f;
    public GameObject freezeEffect;

    void OnCollisionEnter(Collision collision)
    {
        Freeze();
    }

    void Freeze()
    {
        Debug.Log("Iceball shattered at: " + transform.position);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, freezeRadius);
        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsFrozen())
                enemy.Freeze(freezeDuration);
        }

        if (freezeEffect != null)
        {
            ParticleSystem ps = freezeEffect.GetComponent<ParticleSystem>();
            float duration = ps != null ? ps.main.duration : 2f;

            GameObject effect = Instantiate(freezeEffect, transform.position, Quaternion.identity);
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