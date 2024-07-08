using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;
using System.Security.Cryptography;

public class PunchTarget : MonoBehaviour
{
    public string targetPart = "head";
    public int hitDamage = 1;
    public EnemyAI enemy;
    public ChainIKConstraint targetConstraint;
    [SerializeField] private Transform targetBone; // Bone to attach the target to
    [SerializeField] private float launchDistance = 1.0f; // Distance to launch along the hit normal
    [SerializeField] private float launchSpeed = 10.0f; // Speed of launching
    [SerializeField] private float recoveryTime = 1.0f; // Time to recover to initial position
    [SerializeField] private float recoverySpeed = 2.0f; // Speed of target recovery

    private bool isPunched; // Flag to track if target is currently punched
    private float punchTime; // Time when the target was punched

    void FixedUpdate()
    {
        // Smoothly recover target to targetBone's position if not punched
        if (!isPunched)
        {
            transform.position = Vector3.Lerp(transform.position, targetBone.position, recoverySpeed * Time.deltaTime);
        }
        else if (Time.time >= punchTime + recoveryTime)
        {
            // Smoothly move back to the targetBone's position after recoveryTime seconds
            isPunched = false;
            StartCoroutine(SmoothReturn());
        }

        // Periodically set position to targetBone's position if idle
        if (!isPunched && Vector3.Distance(transform.position, targetBone.position) < 0.01f)
        {
            transform.position = targetBone.position;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "PlayerFist" && !isPunched)
        {
            // Calculate launch direction along collision normal
            Vector3 launchDirection = collision.contacts[0].normal;

            // Launch the target along the normal direction
            StartCoroutine(LaunchAndRecover(launchDirection, collision.contacts[0].point));
        }
    }

    IEnumerator LaunchAndRecover(Vector3 launchDirection, Vector3 hitLocation)
    {
        enemy.getDamage(hitDamage, hitLocation, targetPart);
        isPunched = true;
        targetConstraint.weight = 1f;
        punchTime = Time.time;

        // Calculate the launch end position
        Vector3 launchEndPosition = transform.position + launchDirection * launchDistance;

        // Launch the target along the collision normal
        float distanceMoved = 0;
        while (distanceMoved < launchDistance)
        {
            float moveDistance = launchSpeed * Time.deltaTime;
            transform.position += launchDirection * moveDistance;
            distanceMoved += moveDistance;
            yield return null;
        }

        // Ensure the target is exactly at the launch end position
        transform.position = launchEndPosition;
    }

    IEnumerator SmoothReturn()
    {
        targetConstraint.weight = 0f;
        while (Vector3.Distance(transform.position, targetBone.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetBone.position, recoverySpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure the target is exactly at the targetBone's position
        transform.position = targetBone.position;
    }
}
