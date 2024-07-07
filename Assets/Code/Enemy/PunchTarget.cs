using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class PunchTarget : MonoBehaviour
{
    public int hitDamage = 1;
    public EnemyAI enemy;
    public DampedTransform targetConstraint;
    [SerializeField] private Transform targetBone; // Bone to attach the target to
    [SerializeField] private float launchDistance = 1.0f; // Distance to launch along the hit normal
    [SerializeField] private float launchSpeed = 10.0f; // Speed of launching
    [SerializeField] private float recoveryTime = 1.0f; // Time to recover to initial position
    [SerializeField] private float recoverySpeed = 10.0f; // Speed of target recovery

    private Vector3 initialPosition;
    private bool isPunched; // Flag to track if target is currently punched
    private float punchTime; // Time when the target was punched

    void Start()
    {
        isPunched = false;
        UpdatePositionToBone();
        initialPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Smoothly recover target to initial position if not punched
        if (!isPunched)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, recoverySpeed * Time.deltaTime);
        }
        else if (Time.time >= punchTime + recoveryTime)
        {
            // Teleport to initial position after recoveryTime seconds
            transform.position = initialPosition;
            isPunched = false;
            targetConstraint.weight = 0f;
        }
    }

    public void UpdatePositionToBone()
    {
        if (targetBone != null)
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
            StartCoroutine(LaunchAndRecover(launchDirection));
        }
    }

    IEnumerator LaunchAndRecover(Vector3 launchDirection)
    {
        enemy.getDamage(hitDamage);
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
}
