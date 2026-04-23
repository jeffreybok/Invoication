using UnityEngine;

public class CameraFallOnDeath : MonoBehaviour
{
    private Rigidbody rb;
    private bool isFalling = false;

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

        isFalling = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        // 🔥 NO COLLISIONS EVER
        rb.detectCollisions = false;

        // 🔥 PREVENT WEIRD PHYSICS SPIKES
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // 🔥 CRITICAL: kill physics EARLY
    void LateUpdate()
    {
        if (!isFalling) return;

        // if game is about to unload, stop everything instantly
        if (!gameObject.scene.isLoaded)
        {
            DisableFall();
        }
    }

    public void DisableFall()
    {
        if (rb == null) return;

        isFalling = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}