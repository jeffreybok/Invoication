// FireWall.cs — attach this to your FireWall prefab
using System.Collections.Generic;
using UnityEngine;

public class FireWall : MonoBehaviour
{
    [Header("Fire Wall Settings")]
    public float lifetime = 6f;
    public float burnDamagePerTick = 8f;
    public float burnDuration = 4f;
    public float tickRate = 0.5f;

    private readonly HashSet<Enemy> _enemiesInWall = new();
    private float _tickTimer = 0f;

    void Awake()
    {
        _tickTimer = tickRate;
    }

    void Update()
    {
        if (_enemiesInWall.Count == 0) return;

        _tickTimer += Time.deltaTime;
        if (_tickTimer >= tickRate)
        {
            _tickTimer = 0f;
            foreach (Enemy enemy in _enemiesInWall)
            {
                if (enemy != null)
                    enemy.ApplyBurn(burnDamagePerTick, burnDuration);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
            _enemiesInWall.Add(enemy);
    }

    void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
            _enemiesInWall.Remove(enemy);
    }
}