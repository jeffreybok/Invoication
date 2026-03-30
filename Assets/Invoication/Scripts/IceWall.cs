using UnityEngine;
using PurrNet;

public class IceWall : NetworkBehaviour
{
    [Header("Ice Wall Settings")]
    public float lifetime = 6f;
    public float freezeDuration = 3f;
    public float tickRate = 0.5f;

    private float _tickTimer;
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

        _tickTimer += Time.deltaTime;
    }

    void OnCollisionStay(Collision collision)
    {
        if (!isServer) return;

        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null && _tickTimer >= tickRate)
        {
            _tickTimer = 0f;
            enemy.Freeze_Server(freezeDuration, attacker);
        }
    }
}