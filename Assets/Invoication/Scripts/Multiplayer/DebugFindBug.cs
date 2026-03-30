using UnityEngine;

public class DisableTracker : MonoBehaviour
{
    void OnDisable()
    {
        Debug.Log("🚨 PLAYER DISABLED: " + gameObject.name);
        Debug.Log(System.Environment.StackTrace);
    }

    void OnEnable()
    {
        Debug.Log("✅ PLAYER ENABLED: " + gameObject.name);
    }
}