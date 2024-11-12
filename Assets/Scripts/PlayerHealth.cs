using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public const float maxHealth = 100;
    public float health = maxHealth;
    public int deaths = 0;

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnDelay = 3.0f;
    [SerializeField] private Animator animator; 

    private void Awake()
    {
        health = maxHealth;
    }

    public void Start()
    {
        if (IsClient)
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetHealthServerRPC()
    {
        health = maxHealth;
        LocalTakeDamageClientRPC(health);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRPC(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            health = 0;
            Death();
        }

        LocalTakeDamageClientRPC(health);
    }

    private void Death()
    {
        animator.SetTrigger("Death");
        deaths++;
        GetComponent<CharacterController>().enabled = false;

        StartCoroutine(RespawnAfterDeathAnimation());
    }

    private IEnumerator RespawnAfterDeathAnimation()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        yield return new WaitForSeconds(respawnDelay);

        Respawn();
    }

    private void Respawn()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        transform.position = spawnPoint.position;
        health = maxHealth;
        LocalTakeDamageClientRPC(health);

        GetComponent<CharacterController>().enabled = true;
    }

    [ClientRpc]
    private void LocalTakeDamageClientRPC(float newHealth)
    {
        if (!IsOwner)
        {
            return;
        }
        GameUIManager.SetHealthText(newHealth.ToString());
    }
}
