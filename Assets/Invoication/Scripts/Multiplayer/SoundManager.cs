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
    public AudioClip iceWall;

    [Header("Healing")]
    public AudioClip healLoop;

    [Header("UI Audio Clips (LOCAL ONLY)")]
    public AudioClip book;
    public AudioClip select;
    public AudioClip purchase;

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    [Header("3D Sound Settings")]
    public float minDistance = 5f;
    public float maxDistance = 30f;
    public float volume = 1f;

    private AudioSource uiSource;
    private AudioSource musicSource;

    private bool isShuttingDown = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ===============================
        // UI SOUND (2D)
        // ===============================
        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.spatialBlend = 0f;
        uiSource.playOnAwake = false;
        uiSource.volume = 1f;

        // ===============================
        // BACKGROUND MUSIC
        // ===============================
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = 0.2f;
        musicSource.spatialBlend = 0f;

        if (backgroundMusic != null)
            musicSource.Play();
    }

    void OnDestroy()
    {
        isShuttingDown = true;

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

    public void PlayIceWall(Vector3 pos)
    {
        if (!isServer) return;
        PlaySound_ObserversRPC(pos, "iceWall");
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
        if (!Application.isPlaying) return;
        if (!enabled) return;
        if (isShuttingDown) return;

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

        StartCoroutine(DestroyAfterTime(temp, clip.length));
    }

    IEnumerator DestroyAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

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
            case "iceWall": return iceWall;
        }
        return null;
    }
}