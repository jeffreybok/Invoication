using System.Collections.Generic;
using UnityEngine;
using PurrNet;

public class FireWall : NetworkBehaviour
{
    [Header("Fire Wall Settings")]
    public float lifetime = 9f;
    public float burnDamagePerTick = 10f;
    public float burnDuration = 6f;
    public float tickRate = 0.5f;

    private readonly HashSet<Enemy> _enemiesInWall = new();
    private float _tickTimer = 0f;

    private GameObject attacker;

    public void Initialize(GameObject owner)
    {
        attacker = owner;
    }

    void Start()
    {
        if (!isServer) return;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isServer) return;
        if (_enemiesInWall.Count == 0) return;

        _tickTimer += Time.deltaTime;
        if (_tickTimer >= tickRate)
        {
            _tickTimer = 0f;

            foreach (Enemy enemy in _enemiesInWall)
            {
                if (enemy != null)
                    enemy.ApplyBurn_Server(burnDamagePerTick, burnDuration, attacker);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
            _enemiesInWall.Add(enemy);
    }

    void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
            _enemiesInWall.Remove(enemy);
    }
}