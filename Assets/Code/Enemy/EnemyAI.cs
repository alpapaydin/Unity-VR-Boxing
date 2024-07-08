using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public MultiAimConstraint headAim;
    public ChainIKConstraint leftHandChain;
    public ChainIKConstraint rightHandChain;
    public Transform targetPlayer;
    public int hitPoints = 10;
    public float PunchSpeed = 20f;
    public float rotationSpeed = 5f; // Rotation speed
    [SerializeField] private Animator animator; // Reference to the enemy's animator
    [SerializeField] private float attackDelayMin; // Minimum delay between attacks (seconds)
    [SerializeField] private float attackDelayMax; // Maximum delay between attacks (seconds)
    [SerializeField] private float guardChance; // Chance (0-1) of entering guard after an attack
    public GameObject ragdollPrefab; // Assign your ragdoll prefab in the Inspector
    public GameObject[] hitParticles;

    private enum PunchLockMode { off, inc, dec, }
    private PunchLockMode LeftMode = PunchLockMode.off;
    private PunchLockMode RightMode = PunchLockMode.off;
    private bool isDead = false;
    private bool isHit = false;
    private bool isGuarding = false;
    private bool isAttacking; // Flag to track if enemy is currently attacking
    private float nextAttackTime; // Time of the next attack

    private void Start()
    {
        leftHandChain.weight = 0f;
        rightHandChain.weight = 0f;
        nextAttackTime = Time.time + Range(attackDelayMin, attackDelayMax);
    }

    private void Update()
    {
        if (isDead || isHit || isGuarding || isAttacking)
            { return; }
        if (Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        switch (LeftMode)
        {
            case PunchLockMode.inc:
                {
                    leftHandChain.weight = Mathf.Lerp(leftHandChain.weight, 1, PunchSpeed * Time.deltaTime);
                    if (leftHandChain.weight > 0.95f)
                    {
                        leftHandChain.weight = 1;
                        LeftMode = PunchLockMode.dec;
                    }
                    break;
                }
            case PunchLockMode.dec:
                {
                    leftHandChain.weight = Mathf.Lerp(leftHandChain.weight, 0, PunchSpeed * Time.deltaTime);
                    if (leftHandChain.weight < 0.05f)
                    {
                        leftHandChain.weight = 0;
                        LeftMode = PunchLockMode.off;
                    }
                    break;
                }
        }
        switch (RightMode)
        {
            case PunchLockMode.inc:
                {
                    rightHandChain.weight = Mathf.Lerp(rightHandChain.weight, 1, PunchSpeed * Time.deltaTime);
                    if (rightHandChain.weight > 0.95f)
                    {
                        rightHandChain.weight = 1;
                        RightMode = PunchLockMode.dec;
                    }
                    break;
                }
            case PunchLockMode.dec:
                {
                    rightHandChain.weight = Mathf.Lerp(rightHandChain.weight, 0, PunchSpeed * Time.deltaTime);
                    if (rightHandChain.weight < 0.05f)
                    {
                        rightHandChain.weight = 0;
                        RightMode = PunchLockMode.off;
                    }
                    break;
                }
        }
    }

    private void Attack()
    {
        StartCoroutine(RotateToTarget(targetPlayer));
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

    public void punchOut(String direction)
    {
        headAim.weight = 1;
        if (direction == "left")
        {
            leftHandTarget.position = targetPlayer.position;
            LeftMode = PunchLockMode.inc;
        }
        else
        {
            rightHandTarget.position = targetPlayer.position;
            RightMode = PunchLockMode.inc;
        }
    }
    public void punchHit(String direction)
    {
        headAim.weight = 0;
        if (direction == "left")
        {
            LeftMode = PunchLockMode.dec;
        }
        else
        {
            RightMode = PunchLockMode.dec;
        }
    }
    public void animEnd()
    {
        animator.SetTrigger("attackEnd");
        OnAttackEnd();
    }

    public void recoveredHit()
    {
        isHit = false;
        animator.SetInteger("hit", 0);
        OnAttackEnd();
    }

    private bool IsInIdleState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle");
    }

    public void getDamage(int damage, Vector3 hitLocation, String targetPart)
    {
        if (isDead | isHit)
        {
            return;
        }
        isHit = true;
        isAttacking = false;
        RightMode = PunchLockMode.off;
        LeftMode = PunchLockMode.off;
        leftHandChain.weight = 0;
        rightHandChain.weight = 0;
        headAim.weight = 0;
        hitPoints -= damage;
        int randomIndex = Range(0, hitParticles.Length);
        Instantiate(hitParticles[randomIndex], hitLocation, new Quaternion(0,0,0,0));
        switch (targetPart)
        {
            case ("head"):
                {
                    //head hit anim
                    animator.SetInteger("hit", 1);
                    break;
                }
            case ("body"):
                {
                    //body hit
                    animator.SetInteger("hit", 2);
                    break;
                }
        }
        animator.SetTrigger("enemyHit");
        if (hitPoints <= 0)
        {
            ReplaceWithRagdoll();
            isDead = true;
        }
    }

    public void ReplaceWithRagdoll()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        GameObject ragdoll = Instantiate(ragdollPrefab, currentPosition, currentRotation);
        CopyTransformsRecursively(transform, ragdoll.transform);
        Destroy(gameObject);
    }

    private void CopyTransformsRecursively(Transform source, Transform destination)
    {
        destination.position = source.position;
        destination.rotation = source.rotation;
        int childCount = Mathf.Min(source.childCount, destination.childCount);
        for (int i = 0; i < childCount; i++)
        {
            Transform sourceChild = source.GetChild(i);
            Transform destChild = destination.GetChild(i);
            CopyTransformsRecursively(sourceChild, destChild);
        }
    }


    public void OnAttackEnd() // Called at the end of attack animation
    {
        isAttacking = false;
        // Chance to enter guard after attack
        if (value < guardChance)
        {
            animator.SetBool("IsGuarding", true);
            isGuarding = true;
        }

        StartCoroutine(ExitGuardAfterDelay());
    }

    IEnumerator RotateToTarget(Transform target)
    {
        while (true)
        {
            if (target != null)
            {
                Vector3 direction = target.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    private IEnumerator ExitGuardAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);
        animator.SetBool("IsGuarding", false);
        isAttacking = false;
        isGuarding = false;
    }
}
