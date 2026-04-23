using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Transform camTransform;
    private Vector3 originalLocalPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Camera cam = GetComponentInChildren<Camera>();

        if (cam != null)
        {
            camTransform = cam.transform.parent;
            originalLocalPos = camTransform.localPosition;

            Debug.Log("CameraShake initialized on: " + camTransform.name);
        }
        else
        {
            Debug.LogError("No camera found on player!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Shake test");
            Shake(0.5f, 1f);
        }
    }

    public void Shake(float duration, float strength, int vibrato = 10, float randomness = 50f)
    {
        if (camTransform == null) return;

        camTransform.DOKill();

        camTransform.DOShakePosition(
            duration,
            strength,
            vibrato,
            randomness,
            false,
            true
        ).OnComplete(() =>
        {
            camTransform.localPosition = originalLocalPos;
        });
    }

    public void ShakeFromPosition(Vector3 explosionPos, float maxDistance)
    {
        if (camTransform == null) return;

        float dist = Vector3.Distance(Camera.main.transform.position, explosionPos);
        if (dist > maxDistance) return;

        float strength = Mathf.Lerp(3f, 0f, dist / maxDistance);
        float duration = Mathf.Lerp(0.5f, 0.1f, dist / maxDistance);

        Shake(duration, strength);
    }
}