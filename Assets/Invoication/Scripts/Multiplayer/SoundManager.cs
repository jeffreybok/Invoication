using UnityEngine;
using PurrNet;
using System.Collections;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    [Header("Gameplay Audio Clips")]
    public AudioClip fireball;
    public AudioClip explosion;
    public AudioClip shockwave;
    public AudioClip goblinDeath;
    public AudioClip takeDamage;
    public AudioClip iceball;
    public AudioClip firewall;

    [Header("UI Audio Clips (LOCAL ONLY)")]
    public AudioClip book;
    public AudioClip select;
    public AudioClip purchase;

    [Header("3D Sound Settings")]
    public float minDistance = 5f;
    public float maxDistance = 30f;
    public float volume = 1f;

    private AudioSource uiSource;

    // 🔥 GLOBAL BLOCK
    private bool isShuttingDown = false;

    void Awake()
    {
        Instance = this;

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.spatialBlend = 0f;
        uiSource.playOnAwake = false;
        uiSource.volume = 1f;
    }

    void OnDestroy()
    {
        isShuttingDown = true;

        // 🔥 HARD CLEANUP (no delayed destroy allowed)
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var s in sources)
        {
            if (s != null && s.gameObject.name.StartsWith("Sound_"))
            {
                DestroyImmediate(s.gameObject);
            }
        }
    }

    // ===============================
    // GAMEPLAY (NETWORKED)
    // ===============================

    public void PlayFireball(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "fireball");
    }

    public void PlayExplosion(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "explosion");
    }

    public void PlayShockwave(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "shockwave");
    }

    public void PlayGoblinDeath(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "goblinDeath");
    }

    public void PlayDamage(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "damage");
    }

    public void PlayIceball(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "iceball");
    }

    public void PlayFirewall(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "firewall");
    }

    // ===============================
    // UI (LOCAL ONLY)
    // ===============================

    public void PlayBook() => PlayUISound(book);
    public void PlaySelect() => PlayUISound(select);
    public void PlayPurchase() => PlayUISound(purchase);

    void PlayUISound(AudioClip clip)
    {
        if (clip == null) return;

        uiSource.Stop();
        uiSource.PlayOneShot(clip);
    }

    // ===============================
    // NETWORK SOUND SPAWN
    // ===============================

    [ObserversRpc]
    void PlaySound_ObserversRPC(Vector3 pos, string soundName)
    {
        // 🔥 ABSOLUTE BLOCK CONDITIONS
        if (!Application.isPlaying) return;
        if (!enabled) return;
        if (isShuttingDown) return;
        if (!gameObject.scene.isLoaded) return;

        AudioClip clip = GetClip(soundName);
        if (clip == null) return;

        GameObject temp = new GameObject("Sound_" + soundName);
        temp.transform.position = pos;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.volume = volume;

        source.Play();

        // 🔥 SAFE DESTROY (NO UNITY TIMING ISSUES)
        StartCoroutine(DestroyAfterTime(temp, clip.length));
    }

    IEnumerator DestroyAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        // 🔥 BLOCK DESTROY DURING SHUTDOWN
        if (obj == null) yield break;
        if (!Application.isPlaying) yield break;
        if (isShuttingDown) yield break;

        Destroy(obj);
    }

    // ===============================
    // CLIP SELECTOR
    // ===============================

    AudioClip GetClip(string name)
    {
        switch (name)
        {
            case "fireball": return fireball;
            case "explosion": return explosion;
            case "shockwave": return shockwave;
            case "goblinDeath": return goblinDeath;
            case "damage": return takeDamage;
            case "iceball": return iceball;
            case "firewall": return firewall;
        }
        return null;
    }
}