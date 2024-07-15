using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerFist : MonoBehaviour
{
    public string direction = "left";
    public Player player;
    public EnemyAI enemy;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "EnemyFist")
        {
            player.attackBlocked();
        }
    }
}
