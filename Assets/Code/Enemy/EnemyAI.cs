using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public int hitPoints = 10;
    [SerializeField] private Animator animator; // Reference to the enemy's animator
    [SerializeField] private float attackDelayMin; // Minimum delay between attacks (seconds)
    [SerializeField] private float attackDelayMax; // Maximum delay between attacks (seconds)
    [SerializeField] private float guardChance; // Chance (0-1) of entering guard after an attack

    private bool isDead = false;
    private bool isAttacking; // Flag to track if enemy is currently attacking
    private float nextAttackTime; // Time of the next attack

    private void Start()
    {
        nextAttackTime = Time.time + Range(attackDelayMin, attackDelayMax);
    }

    private void Update()
    {
        if (isDead)
            { return; }
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            Attack();
        }
        else if (isAttacking && IsInIdleState())
        {
            OnAttackEnd();
        }
    }

    private void Attack()
    {
        isAttacking = true;

        // Randomly choose between left jab and right hook animations
        int attackType = Range(0, 2);
        if (attackType == 0)
        {
            animator.SetTrigger("LeftJab");
        }
        else
        {
            animator.SetTrigger("RightHook");
        }

        nextAttackTime = Time.time + Range(attackDelayMin, attackDelayMax);
    }

    private bool IsInIdleState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle");
    }

    public void getDamage(int damage)
    {
        if (isDead)
        {
            return;
        }
        hitPoints -= damage;
        if (hitPoints <= 0)
        {
            Console.WriteLine("ded");
            animator.applyRootMotion = false;
            animator.enabled = false;
            isDead = true;
        }
    }

    public void OnAttackEnd() // Called at the end of attack animation
    {
        // Chance to enter guard after attack
        if (value < guardChance)
        {
            animator.SetBool("IsGuarding", true);
        }
        else
        {
            isAttacking = false;
        }

        // Exit guard after a short duration
        StartCoroutine(ExitGuardAfterDelay());
    }

    private IEnumerator ExitGuardAfterDelay()
    {
        yield return new WaitForSeconds(1.0f); // Adjust delay as needed
        animator.SetBool("IsGuarding", false);
        isAttacking = false;
    }
}
