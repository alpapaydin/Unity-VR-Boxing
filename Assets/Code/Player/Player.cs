using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int hp = 50;
    public int damage = 1;
    public float damageCooldown = 2.0f;
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;
    public Transform leftHandGlove;
    public Transform rightHandGlove;
    public PostProcessVolume postProcessVolume;


    private Coroutine lerpCoroutine;
    private bool enemyBlocked = false;
    private bool recentlyDamaged = false;
    private bool isDead = false;

    void Update()
    {
        bool areHandsConnected = OVRPlugin.GetHandTrackingEnabled();
        if (areHandsConnected)
        {
            Debug.Log("Hands Connected: " + areHandsConnected);
            leftHandGlove.localRotation = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
            rightHandGlove.localRotation = new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f);
        }
        else
        {
            Debug.Log("Hands Connected: " + areHandsConnected);
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            leftHandGlove.localRotation = targetRotation;
            rightHandGlove.localRotation = targetRotation;
        }
    }

    public void attackBlocked()
    {
        enemyBlocked = true;
        Invoke("Unblock", 0.2f);
    }

    void Unblock()
    {
        enemyBlocked = false;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "EnemyFist" && !recentlyDamaged && !enemyBlocked)
        {
            getDamage(1);
            Debug.Log("PLAYERhit");
            recentlyDamaged = true;
            StartCoroutine(ResetDamageCooldown());
        }
    }

    public void getDamage(int damage)
    {
        if (isDead)
        {
            return;
        }
        LerpPostProcessWeight();
        hp -= damage;
        if (hp <= 0)
        {
            isDead = true;
        }
    }

    IEnumerator ResetDamageCooldown()
    {
        // Wait for damageCooldown seconds
        yield return new WaitForSeconds(damageCooldown);

        // Reset the recentlyDamaged flag after cooldown expires
        recentlyDamaged = false;
    }

    public void LerpPostProcessWeight()
    {
        if (lerpCoroutine != null)
        {
            StopCoroutine(lerpCoroutine);
        }
        lerpCoroutine = StartCoroutine(LerpWeightCoroutine());
    }

    private IEnumerator LerpWeightCoroutine()
    {
        float elapsedTime = 0f;
        float durationTo1 = 0.1f;
        float durationTo0 = 0.2f;

        // Lerp weight to 1 in 0.1 seconds
        while (elapsedTime < durationTo1)
        {
            postProcessVolume.weight = Mathf.Lerp(0f, 1f, elapsedTime / durationTo1);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        postProcessVolume.weight = 1f;

        // Reset elapsed time for the next lerp
        elapsedTime = 0f;

        // Lerp weight back to 0 in 0.2 seconds
        while (elapsedTime < durationTo0)
        {
            postProcessVolume.weight = Mathf.Lerp(1f, 0f, elapsedTime / durationTo0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        postProcessVolume.weight = 0f;

        lerpCoroutine = null;
    }
}
