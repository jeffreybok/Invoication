using UnityEngine;

public class BossGoblin : MonoBehaviour
{
    [Header("Glow Settings")]
    [Range(0f, 5f)]
    public float glowIntensity = 2f;

    public Color glowTint = new Color(1f, 0f, 0f, 1f); // Red tint overlay
    [Range(0f, 1f)]
    public float tintStrength = 0.4f; // How much red to mix in (0 = no tint, 1 = full red)

    void Start()
    {
        ApplyGlowColor();
    }

    void ApplyGlowColor()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                // Blend existing color with red tint instead of replacing it
                if (mat.HasProperty("_Color"))
                {
                    Color originalColor = mat.color;
                    Color tintedColor = Color.Lerp(originalColor, glowTint, tintStrength);
                    mat.color = tintedColor;
                }

                // Use the material's existing color for emission so details stay visible
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    Color emissionBase = mat.HasProperty("_Color") ? mat.color : glowTint;
                    mat.SetColor("_EmissionColor", emissionBase * glowIntensity);
                }
            }
        }
    }
}