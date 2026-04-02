using UnityEngine;

public class DeathFlicker : MonoBehaviour
{
    public float speed = 3f;
    public Color flickerColor = Color.red;

    private Renderer[] renderers;
    private bool isDead = false;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void StartFlicker()
    {
        isDead = true;
    }

    void Update()
    {
        if (!isDead) return;

        float t = Mathf.PingPong(Time.time * speed, 1f);

        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                mat.EnableKeyword("_EMISSION");

                Color emission = flickerColor * Mathf.Lerp(0.2f, 2f, t);
                mat.SetColor("_EmissionColor", emission);
            }
        }
    }
}