using UnityEngine;
using TMPro;
using Meta.WitAi;
using Oculus.Voice;

public class VoiceSpellCaster : MonoBehaviour
{
    private AppVoiceExperience appVoiceExperience;
    private bool isListening = false;
    private SpellCaster spellCaster;

    [Header("Spell Text Popup")]
    public GameObject textPopupPrefab;
    public Transform textSpawnPoint;
    public float textFloatSpeed = 1f;
    public float textFadeDuration = 2f;

    [Header("Mic Indicator")]
    public UnityEngine.UI.Image micIndicatorImage;
    public Color micOnColor = Color.green;
    public Color micOffColor = Color.red;

    void Start()
    {
        appVoiceExperience = GetComponent<AppVoiceExperience>();
        spellCaster = GetComponent<SpellCaster>();

        if (appVoiceExperience != null)
        {
            appVoiceExperience.TranscriptionEvents.OnFullTranscription.AddListener(OnFullTranscription);
            appVoiceExperience.VoiceEvents.OnStartListening.AddListener(OnStartListening);
            appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);

            Debug.Log("Voice spell casting ready! Press V to toggle microphone.");
        }
        else
        {
            Debug.LogError("AppVoiceExperience not found!");
        }
        UpdateMicIndicator();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            ToggleMicrophone();
    }

    void OnStartListening()
    {
        Debug.Log("🎤 Wit.ai started listening!");
    }

    void OnStoppedListening()
    {
        Debug.Log("🛑 Wit.ai stopped listening!");
    }

    void ToggleMicrophone()
    {
        if (appVoiceExperience == null) return;

        if (isListening)
        {
            appVoiceExperience.Deactivate();
            isListening = false;
        }
        else
        {
            appVoiceExperience.Activate();
            isListening = true;
        }
        UpdateMicIndicator();
    }

    void OnFullTranscription(string transcription)
    {
        Debug.Log("========== You said: " + transcription + " ==========");
        Debug.Log("isListening flag: " + isListening);

        ShowSpellText(transcription);

        string spellSaid = transcription.ToLower().Trim();

        if (spellSaid.Contains("fireball") || spellSaid.Contains("fire ball") || spellSaid.Contains("fire bowl"))
        {
            CastFireball();
        }
        else if (spellSaid.Contains("blazing") || spellSaid.Contains("blazing impact") || spellSaid.Contains("please") || spellSaid.Contains("blaze"))
        {
            CastBlazingImpact();
        }
        else if (spellSaid.Contains("iceball") || spellSaid.Contains("ice ball") || spellSaid.Contains("ace") || spellSaid.Contains("nice"))
        {
            CastIce();
        }
        else if (spellSaid.Contains("lightning"))
        {
            CastLightning();
        }
        else if (spellSaid.Contains("heal"))
        {
            CastHeal();
        }
        else if (spellSaid.Contains("ember") || spellSaid.Contains("ember circle") || spellSaid.Contains("amber"))
        {
            CastEmberCircle();
        }
        else if (spellSaid.Contains("fire wall") || spellSaid.Contains("firewall") || spellSaid.Contains("fire ward"))
        {
            CastFireWall();
        }
        else if (spellSaid.Contains("ice wall") || spellSaid.Contains("icewall") || spellSaid.Contains("ice ward") || spellSaid.Contains("I swallow"))
        {
            CastIceWall();
        }
        else
        {
            Debug.Log("Unknown spell: " + transcription);
        }

        if (isListening)
        {
            Debug.Log("Starting reactivation coroutine...");
            StartCoroutine(ReactivateAfterDelay());
        }
        else
        {
            Debug.LogWarning("NOT reactivating because isListening is FALSE!");
        }
    }

    void ShowSpellText(string text)
    {
        if (textPopupPrefab == null) return;

        Vector3 spawnPos = textSpawnPoint != null
            ? textSpawnPoint.position
            : Camera.main.transform.position + Camera.main.transform.forward * 5f + Vector3.up * 1f;

        GameObject popup = Instantiate(textPopupPrefab, spawnPos, Quaternion.identity);
        TextMeshPro textMesh = popup.GetComponent<TextMeshPro>();

        if (textMesh != null)
            textMesh.text = text.ToUpper();

        StartCoroutine(AnimateText(popup, textMesh));
    }

    private System.Collections.IEnumerator AnimateText(GameObject popup, TextMeshPro textMesh)
    {
        float timer = 0f;
        Color startColor = textMesh != null ? textMesh.color : Color.white;
        Camera mainCamera = Camera.main;

        while (timer < textFadeDuration)
        {
            popup.transform.position += Vector3.up * textFloatSpeed * Time.deltaTime;

            if (mainCamera != null)
                popup.transform.LookAt(popup.transform.position + mainCamera.transform.forward);

            if (textMesh != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / textFadeDuration);
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(popup);
    }

    System.Collections.IEnumerator ReactivateAfterDelay()
    {
        Debug.Log("Waiting 1 second before reactivating...");
        yield return new WaitForSeconds(1f);

        Debug.Log("1 second passed. isListening: " + isListening);

        if (isListening)
        {
            Debug.Log("Calling appVoiceExperience.Activate()...");
            appVoiceExperience.Activate();
            Debug.Log("========== 🟢 MICROPHONE REACTIVATED ==========");
        }
        else
        {
            Debug.LogWarning("Did NOT reactivate because isListening became false!");
        }
    }

    void CastFireball()
    {
<<<<<<< HEAD
        if (spellCaster != null) spellCaster.CastFireball();
        else Debug.LogError("SpellCaster not found!");
=======
        if (!SkillTreeBridge.IsUnlocked("Fireball_0"))
        {
            ShowSpellText("Fireball spell locked");
            return;
        }
        
        if (spellCaster != null)
            spellCaster.CastFireball();
        else
            Debug.LogError("SpellCaster not found!");
>>>>>>> main
    }

    void CastBlazingImpact()
    {
<<<<<<< HEAD
        if (spellCaster != null) spellCaster.CastBlazingImpact();
        else Debug.LogError("SpellCaster not found!");
=======
        if (!SkillTreeBridge.IsUnlocked("BlazingImpact_0"))
        {
            ShowSpellText("Blazing Impact spell locked");
            return;
        }
        
        if(spellCaster != null)
            spellCaster.CastBlazingImpact();
        else
            Debug.LogError("SpellCaster not found!");
>>>>>>> main
    }

    void CastIce()
    {
<<<<<<< HEAD
        if (spellCaster != null) spellCaster.CastIceball();
        else Debug.LogError("SpellCaster not found!");
=======
        if (!SkillTreeBridge.IsUnlocked("IceSpike_0"))
        {
            ShowSpellText("Ice Spike spell locked");
            return;
        }
        
        if (spellCaster != null)
            spellCaster.CastIceball();
        else
            Debug.LogError("SpellCaster not found!");
>>>>>>> main
    }

    void CastLightning()
    {
        Debug.Log("LIGHTNING SPELL CAST!");
    }

    void CastHeal()
    {
        Debug.Log("HEAL CAST!");
    }

    void CastFireWall()
    {
        if (spellCaster != null) spellCaster.CastFireWall();
        else Debug.LogError("SpellCaster not found!");
    }

    void CastIceWall()
    {
        if (spellCaster != null) spellCaster.CastIceWall();
        else Debug.LogError("SpellCaster not found!");
    }

    void CastEmberCircle()
    {
        if (spellCaster != null) spellCaster.CastEmberCircle();
        else Debug.LogError("SpellCaster not found!");
    }

    void OnDestroy()
    {
        if (appVoiceExperience != null)
        {
            appVoiceExperience.TranscriptionEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
            appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
            appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
        }
    }

    void UpdateMicIndicator()
    {
        if (micIndicatorImage != null)
            micIndicatorImage.color = isListening ? micOnColor : micOffColor;
    }
}