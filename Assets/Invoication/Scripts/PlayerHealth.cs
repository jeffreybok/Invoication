using UnityEngine;
using UnityEngine.UI;
using PurrNet;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    public bool isDead = false;

    [Header("UI")]
    public Text healthText;
    public Slider healthSlider;
    public Image healthFill;
    public GameObject youDiedUI;

    [Header("Freeze Settings")]
    public Color frozenColor = new Color(0.4f, 0.9f, 1f); // Matches goblin frozen color

    public bool isFrozen = false;

    private Renderer[] renderers;
    private Color[][] originalColors;
    private Coroutine freezeRoutine;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (youDiedUI != null)
            youDiedUI.SetActive(false);

        CacheRenderersAndColors();
        UpdateUI();
    }

    // =========================
    // RENDERER CACHE
    // =========================

    void CacheRenderersAndColors()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = new Color[renderers[i].materials.Length];

            for (int j = 0; j < renderers[i].materials.Length; j++)
                originalColors[i][j] = renderers[i].materials[j].color;
        }
    }

    // =========================
    // DAMAGE (SERVER ONLY)
    // =========================

    public void TakeDamage(float dmg)
    {
        if (!isServer) return;
        if (isDead) return;
        if (isFrozen) return; // frozen players can't take damage (optional — remove if you want them to)

        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SoundManager.Instance.PlayDamage(transform.position);

        var anim = GetComponent<PlayerAnimationController>();
        if (anim != null)
            anim.PlayHit();

        UpdateUI_ObserversRPC(currentHealth);

        if (currentHealth <= 0)
            Die_Server();
    }

    // =========================
    // HEAL (SERVER ONLY)
    // =========================

    public void Heal(float amount)
    {
        if (!isServer) return;
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI_ObserversRPC(currentHealth);
    }

    // =========================
    // FREEZE
    // =========================

    public void Freeze(float duration)
    {
        if (isServer)
            FreezeInternal(duration);
        else
            Freeze_ServerRPC(duration);
    }

    [ServerRpc(requireOwnership: false)]
    void Freeze_ServerRPC(float duration)
    {
        FreezeInternal(duration);
    }

    void FreezeInternal(float duration)
    {
        if (!isServer) return;
        if (isDead) return;
        if (duration <= 0f) return;

        isFrozen = true;

        // Stop player movement
        var controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;

        // Pause animation
        var anim = GetComponent<Animator>();
        if (anim != null)
            anim.speed = 0f;

        RefreshVisuals_ObserversRPC(true);

        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeDurationCoroutine(duration));
    }

    IEnumerator FreezeDurationCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        UnfreezeInternal();
        freezeRoutine = null;
    }

    void UnfreezeInternal()
    {
        if (!isServer) return;
        if (isDead) return;

        isFrozen = false;

        var controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = true;

        var anim = GetComponent<Animator>();
        if (anim != null)
            anim.speed = 1f;

        RefreshVisuals_ObserversRPC(false);
    }

    // =========================
    // DEATH (SERVER)
    // =========================

    void Die_Server()
    {
        if (isDead) return;

        isDead = true;

        // Cancel freeze on death
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        isFrozen = false;
        RefreshVisuals_ObserversRPC(false);

        var anim = GetComponent<PlayerAnimationController>();
        if (anim != null)
            anim.PlayDeath();

        StartFlicker_ObserversRPC();
        Die_ObserversRPC();

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.CheckAllPlayersDead();
    }

    // =========================
    // VISUALS (ALL CLIENTS)
    // =========================

    [ObserversRpc]
    void RefreshVisuals_ObserversRPC(bool frozen)
    {
        ApplyVisualStateLocal(frozen);
    }

    void ApplyVisualStateLocal(bool frozen)
    {
        if (renderers == null || renderers.Length == 0)
            CacheRenderersAndColors();

        RestoreOriginalColors();

        if (frozen)
            ApplyColorToAllRenderers(frozenColor);
    }

    void ApplyColorToAllRenderers(Color color)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
                mats[j].color = color;
        }
    }

    void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            if (i >= originalColors.Length) continue;

            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (j >= originalColors[i].Length) continue;
                mats[j].color = originalColors[i][j];
            }
        }
    }

    // =========================
    // FLICKER (ALL CLIENTS)
    // =========================

    [ObserversRpc]
    void StartFlicker_ObserversRPC()
    {
        var flicker = GetComponent<DeathFlicker>();
        if (flicker != null)
            flicker.StartFlicker();
    }

    // =========================
    // CLIENT DEATH (OWNER ONLY)
    // =========================

    [ObserversRpc]
    void Die_ObserversRPC()
    {
        if (!isOwner) return;

        Debug.Log("You died");

        if (youDiedUI != null)
            youDiedUI.SetActive(true);

        var controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;

        int hiddenLayer = LayerMask.NameToLayer("DeadHidden");

// change this player + all children
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = hiddenLayer;
        }

        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            int layer = LayerMask.NameToLayer("DeadHidden");
            cam.cullingMask &= ~(1 << layer);
        }

        CameraFallOnDeath camFall = GetComponentInChildren<CameraFallOnDeath>();
        camFall.EnableFall();

    }

    // =========================
    // UI SYNC (ALL CLIENTS)
    // =========================

    [ObserversRpc]
    void UpdateUI_ObserversRPC(float newHealth)
    {
        currentHealth = newHealth;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = "HP: " + currentHealth.ToString("0") + " / " + maxHealth.ToString("0");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthFill != null)
            healthFill.color = Color.red;
    }
}