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
    [SerializeField] private Rigidbody2D rb2d;
  
    public GameObject prefab;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.isKinematic = false;
    }

    public void Init(Vector3 direction)
    {
        
        moveDirection = direction;
        moveDirection.z = 0f;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = IsServer;
        if(IsServer)
            Invoke(nameof(SelfDestroy), LifeSpan);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(!NetworkManager.Singleton.IsServer) return;
        
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

    private void FixedUpdate()
    {
        if (!IsServer) return;
        rb2d.velocity = moveDirection * moveSpeed;
    }

    
}
