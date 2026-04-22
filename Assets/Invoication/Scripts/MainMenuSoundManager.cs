using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MainMenuSoundManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Start()
    {
        SetupAudio();
        SetupButtons();
    }

    void SetupAudio()
    {
        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.playOnAwake = true;
        musicSource.volume = 0.5f;
        musicSource.Play();

        // SFX source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1f;
    }

    void SetupButtons()
    {
        // Get ALL buttons, including inactive ones
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        // ALSO get inactive ones under Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        List<Button> allButtons = new List<Button>();

        foreach (var b in buttons)
            allButtons.Add(b);

        foreach (Canvas c in canvases)
        {
            Button[] childButtons = c.GetComponentsInChildren<Button>(true); // TRUE = include inactive

            foreach (Button b in childButtons)
            {
                if (!allButtons.Contains(b))
                    allButtons.Add(b);
            }
        }

        foreach (Button btn in allButtons)
        {
            AddHoverSound(btn);
            AddClickSound(btn);
        }
    }

    void AddClickSound(Button btn)
    {
        btn.onClick.AddListener(() =>
        {
            PlayClick();
        });
    }

    void AddHoverSound(Button btn)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();

        // prevent duplicate entries
        foreach (var t in trigger.triggers)
        {
            if (t.eventID == EventTriggerType.PointerEnter)
                return;
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;

        entry.callback.AddListener((eventData) =>
        {
            PlayHover();
        });

        trigger.triggers.Add(entry);
    }

    void PlayHover()
    {
        if (hoverSound != null)
            sfxSource.PlayOneShot(hoverSound);
    }

    void PlayClick()
    {
        if (clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }
}