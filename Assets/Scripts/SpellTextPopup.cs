using UnityEngine;
using TMPro;

public class SpellTextPopup : MonoBehaviour
{
    public GameObject textPopupPrefab;
    public Transform spawnPoint; // Where the text appears (e.g., above player or at staff tip)
    public float floatSpeed = 1f;
    public float fadeDuration = 2f;
    
    public void ShowSpellText(string spellName)
    {
        if (textPopupPrefab == null || spawnPoint == null) return;
        
        GameObject popup = Instantiate(textPopupPrefab, spawnPoint.position, Quaternion.identity);
        SpellTextAnimation anim = popup.GetComponent<SpellTextAnimation>();
        
        if (anim != null)
        {
            anim.Initialize(spellName, floatSpeed, fadeDuration);
        }
    }
}