using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int hp = 50;
    public int damage = 1;
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;
    public Transform leftHandGlove;
    public Transform rightHandGlove;

    private bool recentlyDamaged = false;
    private bool isDead = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "EnemyFist" && !recentlyDamaged)
        {
            getDamage(1);
            recentlyDamaged = true;
        }
    }


    public void getDamage(int damage)
    {
        if (isDead)
        {
            return;
        }
        hp -= damage;
        if (hp <= 0)
        {
            Console.WriteLine("ded");
            isDead = true;
        }
    }
}
