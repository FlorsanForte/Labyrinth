using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public const float maxHealth = 100;
    public float health = maxHealth;
    public void TakeDamage(float amount){
        if(!IsServer){
            return;
        }
        health -= amount;
        if (health < 0){
            health = 0;
            Debug.Log("Dead");
        }
    }
}
