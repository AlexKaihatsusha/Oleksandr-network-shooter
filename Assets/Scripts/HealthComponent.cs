using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private int MaxHealth = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(0);
    private GameObject owner;
    public delegate void OnDeath();
    public event OnDeath delegateOnDeath;
     
    private void Start()
    {
        currentHealth.OnValueChanged += OnHealthChanged;
        // Debug.Log($"HealthComponent: set function Death, TakeDamage");

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentHealth.Value = MaxHealth;
        }
        
        
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (previousValue > 0 && newValue <= 0)
        {
            HandleDealth();
        }
    }
    public void Init(GameObject owner)
    {
        this.owner = owner;
    }

    public void TakeDamage(int DamageAmount, ulong damageCauser)
    {
        if (IsServer)
        {

            if (NetworkManager.Singleton.LocalClientId == damageCauser) return;
            if (currentHealth.Value > 0)
            {
                //Debug.Log($"HealthComponent: {gameObject} took damage - {DamageAmount}");
                currentHealth.Value -= DamageAmount;
                if (currentHealth.Value <= 0)
                {
                    HandleDealth();
                }
            }
            else
            {
                HandleDealth();
            }
        }
    }


    
    private void HandleDealth()
    {
        delegateOnDeath?.Invoke();
        if (IsServer)
        {
            Debug.Log($"disable object {this.gameObject}");
        }
    }
   
}
