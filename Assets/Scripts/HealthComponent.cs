using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private int MaxHealth = 100;
    private int currentHealth = 0;
    private GameObject owner;
    delegate void OnDeath();
    OnDeath delegateOnDeath;
     
    private void Start()
    {
        delegateOnDeath += Death;
        currentHealth = MaxHealth;
        // Debug.Log($"HealthComponent: set function Death, TakeDamage");
        
    }
    
    public override void OnNetworkSpawn()
    {
    }

    public void Init(GameObject owner)
    {
        this.owner = owner;
    }
    public void TakeDamage(int DamageAmount, GameObject DamageCauser)
    {
        if (gameObject == DamageCauser) return;
        if (currentHealth > 0)
        {
            Debug.Log($"HealthComponent: {gameObject} took damage - {DamageAmount}");
            currentHealth -= DamageAmount;
            if (currentHealth <= 0)
            {
                delegateOnDeath();
            }
        }
        else
        {
            Debug.Log($"HealthComponent: {gameObject} Health below equal zero!");
            delegateOnDeath();
        }
    }

    private void Death()
    {
        Debug.Log($"HealthComponent: {transform.gameObject} has been destroyed");
        
        /*Player playerRef = owner.gameObject.GetComponent<Player>();
        if (playerRef != null)
        {
            playerRef.delegateOnDeath();
        }*/

    }
}
