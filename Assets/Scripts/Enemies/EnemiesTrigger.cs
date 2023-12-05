using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesTrigger : MonoBehaviour
{
    public Door door;
    public GameObject[] enemies;
    public Transform[] spwanPoints;
    
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !triggered)
        {
            triggered = true;
            door.CloseDoor();
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            Instantiate(enemies[i], spwanPoints[i]);
        }
    }
}
