using System;
using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;
    
    [Header("References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI costText;

    private Vector3 offset = new Vector3(160f, 120f, 0f);
    private Vector3 originalPosition = new Vector3(1141f, -370f, 0);
    
    void Awake()
    {
        Instance = this;
        tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        transform.position = Input.mousePosition + offset;
    }

    public void Show(string title, string desc, int cost)
    {
        titleText.text = title;
        infoText.text = desc;
        costText.text = cost.ToString();
        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
        transform.position = originalPosition;
    }
}
