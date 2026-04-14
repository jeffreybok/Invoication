using UnityEngine;
using TMPro;

public class SpellTextPopup : MonoBehaviour
{
    public GameObject textPopupPrefab;
    public Transform spawnPoint;
    public float floatSpeed = 1f;
    public float fadeDuration = 2f;
    public bool enabled = true; // uncheck in Inspector to hide popups

    public void ShowSpellText(string spellName)
    {
        if (!enabled) return;
        if (textPopupPrefab == null || spawnPoint == null) return;

        GameObject popup = Instantiate(textPopupPrefab, spawnPoint.position, Quaternion.identity);
        TextMeshPro textMesh = popup.GetComponent<TextMeshPro>();

        if (textMesh != null)
            textMesh.text = spellName.ToUpper();

        StartCoroutine(AnimateText(popup, textMesh));
    }

    private System.Collections.IEnumerator AnimateText(GameObject popup, TextMeshPro textMesh)
    {
        float timer = 0f;
        Color startColor = textMesh != null ? textMesh.color : Color.white;
        Camera mainCamera = Camera.main;

        while (timer < fadeDuration)
        {
            popup.transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            if (mainCamera != null)
                popup.transform.LookAt(popup.transform.position + mainCamera.transform.forward);

            if (textMesh != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(popup);
    }
}