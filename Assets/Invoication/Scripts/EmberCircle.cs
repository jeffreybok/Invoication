using UnityEngine;
using PurrNet;
using System.Collections;

public class EmberCircle : NetworkBehaviour
{
    public float healPerTick = 5f;
    public float tickRate = 0.5f;
    public float radius = 4f;

    void Start()
    {
        if (!isServer) return;

        StartCoroutine(HealLoop());
    }

    IEnumerator HealLoop()
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider col in hits)
            {
                PlayerHealth player = col.GetComponent<PlayerHealth>();
                if (player != null && !player.isDead)
                {
                    player.Heal(healPerTick);
                }
            }

            yield return new WaitForSeconds(tickRate);
        }
    }
}