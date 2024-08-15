using System;
using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.Infrastructure;
using Unity.Netcode;
using UnityEngine;

public class Meteor : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int damageAmount = 20;
    private Vector3 moveDirection = Vector3.zero;
    [SerializeField] private float LifeSpan = 6f;
  
    public GameObject prefab;
    public void Init(Vector3 direction)
    {
        
        moveDirection = direction;
        moveDirection.z = 0f;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = IsServer;
        Invoke(nameof(SelfDestroy), LifeSpan);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var otherGameObjectHealthComponent = other.gameObject.GetComponent<HealthComponent>();
        
        if (otherGameObjectHealthComponent != null)
        {
            otherGameObjectHealthComponent.TakeDamage(damageAmount, NetworkObjectId);
            SelfDestroy();
        }
        else
        {
            SelfDestroy();
        }
    }
    private void SelfDestroy()
    {
        if (!NetworkObject.IsSpawned)
        {
            return;
        }
        NetworkObject.Despawn(true);
        
    }

    void Update()
    {
        //since we spawn it from Emitter and Emitter is Server authority
        if (!IsServer) return;
            transform.position += moveDirection * (moveSpeed * Time.deltaTime);
           
    }
}
