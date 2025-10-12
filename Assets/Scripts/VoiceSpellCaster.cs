using UnityEngine;
using Meta.WitAi;
using Oculus.Voice;

public class VoiceSpellCaster : MonoBehaviour
{
    private AppVoiceExperience appVoiceExperience;
    private bool isListening = false;
    private FireballCaster fireballCaster;

    void Start()
    {
        appVoiceExperience = GetComponent<AppVoiceExperience>();
        fireballCaster = GetComponent<FireballCaster>();
        
        if (appVoiceExperience != null)
        {
            appVoiceExperience.TranscriptionEvents.OnFullTranscription.AddListener(OnFullTranscription);
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

    void ToggleMicrophone()
    {
        if (appVoiceExperience == null) return;
        
        // Check if actually active
        if (appVoiceExperience.Active)
        {
            // Turn off
            appVoiceExperience.Deactivate();
            isListening = false;
            Debug.Log("Microphone OFF");
        }
        else
        {
            // Turn on
            appVoiceExperience.Activate();
            isListening = true;
            Debug.Log("Microphone ON - Listening...");
        }
    }

    void OnFullTranscription(string transcription)
    {
        Debug.Log("You said: " + transcription);
    
        string spellSaid = transcription.ToLower().Trim();
        
        if (spellSaid.Contains("fireball") || spellSaid.Contains("fire ball"))
        {
            CastFireball();
        }
        else if (spellSaid.Contains("ice"))
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

        // Resume listening after spell if it was on
        if (isListening)
        {
            StartCoroutine(ReactivateAfterDelay());
        }
    }

    System.Collections.IEnumerator ReactivateAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (isListening && !appVoiceExperience.Active)
        {
            appVoiceExperience.Activate();
            Debug.Log("Resuming listening...");
        }
    }

    void CastFireball()
    {
        if (fireballCaster != null)
        {
            fireballCaster.CastFireball();
        }
        else
        {
            Debug.LogError("FireballCaster not found!");
        }
    }

    void CastIce()
    {
        Debug.Log("ICE SPELL CAST!");
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
        }
    }
}