using UnityEngine;

public class TowerAnimator : MonoBehaviour
{
    private Animator animator;
    private Tower tower;
    private float attackDuration = 0.5f; // Duração da animação de ataque
    private float attackCooldown = 0f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        tower = GetComponent<Tower>();
    }

    private void Update()
    {
        // Reduz o cooldown
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        // Se terminou o cooldown, volta para Idle
        if (attackCooldown <= 0 && animator != null)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    public void PlayShootAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking", true);  // ✅ MUDOU AQUI
            attackCooldown = attackDuration;
        }
    }
}