using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private GameObject Owner;
    [SerializeField] private int MaxHealth = 100;
    private int currentHealth = 0;
    
    delegate void OnDeath();
    OnDeath delegateOnDeath;
     
    private void Start()
    {
        delegateOnDeath += Death;
       
        Debug.Log($"HealthComponent: set function Death, TakeDamage");
        
    }

    public override void OnNetworkSpawn()
    {
        currentHealth = MaxHealth;
        Debug.Log($"HealthComponent: set current health(NETWORK)");
    }
    
    public void TakeDamage(int DamageAmount,  GameObject DamageCauser)
    {
        if (Owner == DamageCauser) return;
        if (currentHealth !<= 0)
        {
            Debug.Log($"HealthComponent: {Owner} took damage - {DamageAmount}");
            currentHealth -= DamageAmount;
        }
        else
        {
            Debug.Log($"HealthComponent: {Owner} Health below equal zero!");
            delegateOnDeath();
        }
    }

    private void Death()
    {
        Debug.Log($"HealthComponent: {Owner} has been destroyed");
        Destroy(Owner);
    }
}
