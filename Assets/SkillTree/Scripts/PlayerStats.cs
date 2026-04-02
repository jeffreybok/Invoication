using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player")]
    public float baseMana = 100f;
    public float currentMana;
    public int skillPoints;

    void Start()
    {
        currentMana = baseMana;
    }

    public void ResetToBase()
    {
        currentMana = baseMana;
    }
}