// ===============================
// SoundManager.cs
// ===============================
// MULTIPLAYER SOUND MANAGER (PurrNet)
//
// - 3D GAMEPLAY SOUNDS → server → all players
// - UI SOUNDS (book/select/purchase) → LOCAL ONLY (2D)
//
// ===============================

using UnityEngine;
using PurrNet;

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

    void Awake()
    {
        Instance = this;

        // local UI audio source (2D)
        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.spatialBlend = 0f; // 2D
        uiSource.playOnAwake = false;
        uiSource.volume = 1f;
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
    // UI (LOCAL ONLY - NO RPC)
    // ===============================

    public void PlayBook()
    {
        PlayUISound(book);
    }

    public void PlaySelect()
    {
        PlayUISound(select);
    }

    public void PlayPurchase()
    {
        PlayUISound(purchase);
    }

    void PlayUISound(AudioClip clip)
    {
        if (clip == null) return;

        // stop previous to prevent stacking distortion
        uiSource.Stop();

        uiSource.PlayOneShot(clip);
    }

    // ===============================
    // NETWORK SYNC (GAMEPLAY ONLY)
    // ===============================

    [ObserversRpc]
    void PlaySound_ObserversRPC(Vector3 pos, string soundName)
    {
        AudioClip clip = GetClip(soundName);
        if (clip == null) return;

        GameObject temp = new GameObject("Sound_" + soundName);
        temp.transform.position = pos;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f; // 3D
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.volume = volume;

        source.Play();

        Destroy(temp, clip.length);
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