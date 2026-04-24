using UnityEngine;
using PurrNet;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : NetworkBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateMovement();

        if (!isOwner) return;
    }

    void UpdateMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float speed = new Vector2(moveX, moveZ).magnitude;

        animator.SetFloat("Speed", speed);
    }

    // =========================
    // ATTACK (SERVER → ALL)
    // =========================
    public void PlayAttack()
    {
        if (isOwner)
            PlayAttack_ServerRPC();
    }

    [ObserversRpc]
    void Attack_ObserversRPC()
    {
        animator.SetTrigger("Attack");
    }

    // =========================
    // GET HIT (SERVER → ALL)
    // =========================
    public void PlayHit()
    {
        if (!isServer) return;

        Hit_ObserversRPC();
    }

    [ObserversRpc]
    void Hit_ObserversRPC()
    {
        animator.SetTrigger("GetHit");
    }

    // =========================
    // DIE (SERVER → ALL)
    // =========================
    public void PlayDeath()
    {
        if (!isServer) return;

        Death_ObserversRPC();
    }

    [ObserversRpc]
    void Death_ObserversRPC()
    {
        animator.SetTrigger("Die");
    }
    
    [ServerRpc]
    public void PlayAttack_ServerRPC()
    {
        PlayAttack();
    }
}