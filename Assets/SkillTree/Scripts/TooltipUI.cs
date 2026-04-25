using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI costText;

    private Vector3 offset = new Vector3(160f, 120f, 0f);
    private Vector3 originalPosition;
    
    void Awake()
    {
        originalPosition = transform.position;
        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            transform.position = Input.mousePosition + offset;
        }
    }

    public void Show(string title, string desc, int cost)
    {
        titleText.text = title;
        infoText.text = desc;
        costText.text = cost.ToString();
        
        transform.position = Input.mousePosition + offset;
        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
        transform.position = originalPosition;
    }
}