using UnityEngine;
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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleMicrophone();
        }
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
            Debug.Log("========== 🔴 MICROPHONE OFF ==========");
        }
        else
        {
            appVoiceExperience.Activate();
            isListening = true;
            Debug.Log("========== 🟢 MICROPHONE ON - LISTENING ==========");
        }
    }

    void OnFullTranscription(string transcription)
    {
        Debug.Log("========== You said: " + transcription + " ==========");
        Debug.Log("isListening flag: " + isListening);
        
        // Show whatever was said as floating text
        ShowSpellText(transcription);
    
        string spellSaid = transcription.ToLower().Trim();
        
        if (spellSaid.Contains("fire") || spellSaid.Contains("fireball") || spellSaid.Contains("fire ball") || spellSaid.Contains("fire bowl"))
        {
            CastFireball();
        }
        else if (spellSaid.Contains("ice") || spellSaid.Contains("iceball") || spellSaid.Contains("ice ball") || spellSaid.Contains("ace") || spellSaid.Contains("nice"))
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
        
        Vector3 spawnPos;
        if (textSpawnPoint != null)
        {
            spawnPos = textSpawnPoint.position;
        }
        else
        {
            spawnPos = Camera.main.transform.position + 
                       Camera.main.transform.forward * 2f + 
                       Vector3.up * 1f;
        }
        
        GameObject popup = Instantiate(textPopupPrefab, spawnPos, Quaternion.identity);
        SpellTextAnimation anim = popup.GetComponent<SpellTextAnimation>();
        
        if (anim != null)
        {
            anim.Initialize(text, textFloatSpeed, textFadeDuration);
        }
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
        if (spellCaster != null)
        {
            spellCaster.CastFireball();
        }
        else
        {
            Debug.LogError("SpellCaster not found!");
        }
    }

    void CastIce()
    {
        if (spellCaster != null)
        {
            spellCaster.CastIceball();
        }
        else
        {
            Debug.LogError("SpellCaster not found!");
        }
    }

    void CastLightning()
    {
        Debug.Log("LIGHTNING SPELL CAST!");
    }

    void CastHeal()
    {
        Debug.Log("HEAL CAST!");
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
}