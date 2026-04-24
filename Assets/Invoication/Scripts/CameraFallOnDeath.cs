using UnityEngine;
using System.Collections;

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
        StartCoroutine(StopAfterTime(0.4f));
        
        if (rb == null) return;

        rb.isKinematic = false;
        rb.useGravity = true;

        // 🔥 keep this OFF so it doesn't freak out / clip / bounce
        rb.detectCollisions = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }
    
    IEnumerator StopAfterTime(float t)
    {
        yield return new WaitForSeconds(t);

        if (rb == null) yield break;

        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;
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