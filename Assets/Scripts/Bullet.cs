using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float life = 3; 
    public ulong attackerID = 0;
    private void Start()
    {
        if (IsServer)
        {
            Destroy(gameObject, life);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            GameObject hit = collision.gameObject;
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null){
                health.TakeDamageServerRPC(50);
            }
            Destroy(gameObject);
        }
    }
}
