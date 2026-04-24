using UnityEngine;
using PurrNet;

public class FinishCollider : NetworkBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.TriggerWin();
            }
        }
    }
}