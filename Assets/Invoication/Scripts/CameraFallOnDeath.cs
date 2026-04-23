using UnityEngine;

public class CameraFallOnDeath : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void EnableFall()
    {
        if (rb == null) return;

        rb.isKinematic = false;
        rb.useGravity = true;

        // 🔥 prevents crazy collision spam
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // 🔥 called before scene change
    public void DisableFall()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}