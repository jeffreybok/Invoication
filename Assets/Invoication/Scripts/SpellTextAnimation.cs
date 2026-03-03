using UnityEngine;
using TMPro;

public class SpellTextAnimation : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float floatSpeed;
    private float fadeDuration;
    private float timer = 0f;
    private Color startColor;
    private Camera mainCamera;
    
    public void Initialize(string text, float speed, float duration)
    {
        textMesh = GetComponent<TextMeshPro>();
        mainCamera = Camera.main;
        floatSpeed = speed;
        fadeDuration = duration;
        
        if (textMesh != null)
        {
            textMesh.text = text.ToUpper();
            startColor = textMesh.color;
        }
        
        Destroy(gameObject, fadeDuration);
    }
    
    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
        
        timer += Time.deltaTime;
        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }
}