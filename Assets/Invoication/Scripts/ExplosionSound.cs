using UnityEngine;

public class ExplosionSound : MonoBehaviour
{
    private bool played = false;

    void Start()
    {
        if (played) return;
        played = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayExplosion(transform.position);
        }
        else
        {
            Debug.LogWarning("[ExplosionSound] No SoundManager found.");
        }
    }
}