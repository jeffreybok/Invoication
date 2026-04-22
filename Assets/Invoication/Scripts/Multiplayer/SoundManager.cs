// ===============================
// SoundManager.cs
// ===============================
// SIMPLE MULTIPLAYER SOUND MANAGER (PurrNet)
//
// HOW TO USE:
// 1. Put this on a GameObject in scene (SoundManager)
// 2. Drag your AudioClips into slots
// 3. Call from ANY script:
//      SoundManager.Instance.PlayFireball(position);
//
// MUST be called from SERVER (or call ServerRPC first)
//
// ===============================

using UnityEngine;
using PurrNet;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Clips")]
    public AudioClip fireball;
    public AudioClip explosion;
    public AudioClip shockwave;
    public AudioClip goblinDeath;
    public AudioClip takeDamage;
    public AudioClip iceball;
    public AudioClip firewall;

    [Header("3D Sound Settings")]
    public float minDistance = 5f;
    public float maxDistance = 30f;
    public float volume = 1f;

    void Awake()
    {
        Instance = this;
    }

    // ===============================
    // PUBLIC PLAY FUNCTIONS
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
    // NETWORK SYNC
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
        source.spatialBlend = 1f; // 3D sound
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