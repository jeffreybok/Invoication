using UnityEngine;

public class IceWall : MonoBehaviour
{
    [Header("Ice Wall Settings")]
    public float lifetime = 6f;
    public float freezeDuration = 3f;
    public float tickRate = 0.5f;

    private float _tickTimer;

    void Awake()
    {
        _tickTimer = tickRate;
    }

    void Update()
    {
        _tickTimer += Time.deltaTime;
    }

    void OnCollisionStay(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null && _tickTimer >= tickRate)
        {
            _tickTimer = 0f;
            enemy.Freeze(freezeDuration);
        }
    }
}