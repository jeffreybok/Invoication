using UnityEngine;
using PurrNet;
using System.Collections;

public class EmberCircle : NetworkBehaviour
{
    public float healPerTick = 5f;
    public float tickRate = 0.5f;
    public float radius = 4f;

    private AudioSource healSource;
    private Transform localPlayer;

    void Start()
    {
        if (isServer)
            StartCoroutine(HealLoop());

        // find local player
        if (isOwner)
            localPlayer = transform;
        else
        {
            var players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p.isOwner)
                {
                    localPlayer = p.transform;
                    break;
                }
            }
        }

        // setup audio (but DO NOT PLAY yet)
        healSource = gameObject.AddComponent<AudioSource>();
        healSource.clip = SoundManager.Instance.healLoop;
        healSource.loop = true;
        healSource.spatialBlend = 0f;
        healSource.volume = 0.4f;
    }

    void Update()
    {
        if (localPlayer == null || healSource == null) return;

        float dist = Vector3.Distance(localPlayer.position, transform.position);

        if (dist <= radius)
        {
            if (!healSource.isPlaying)
                healSource.Play();
        }
        else
        {
            if (healSource.isPlaying)
                healSource.Stop();
        }
    }

    IEnumerator HealLoop()
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider col in hits)
            {
                PlayerHealth player = col.GetComponent<PlayerHealth>();
                if (player != null && !player.isDead)
                {
                    player.Heal(healPerTick);
                }
            }

            yield return new WaitForSeconds(tickRate);
        }
    }

    void OnDestroy()
    {
        if (healSource != null)
            healSource.Stop();
    }
}