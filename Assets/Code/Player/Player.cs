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

    private bool recentlyDamaged = false;
    private bool isDead = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
