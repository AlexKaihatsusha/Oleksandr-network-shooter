using System;
using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.Infrastructure;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    private GameObject owner;
    [SerializeField] private float LifeSpan = 2f;
    
    [SerializeField] public float BulletMoveSpeed = 3f;
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] private Rigidbody2D rb2d;
 
    
    private void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(SelfDestroy), LifeSpan);
        }
        if(rb2d == null)
            rb2d = GetComponent<Rigidbody2D>();

        rb2d.isKinematic = false;
    }
    
    public void Init(GameObject owner,Vector3 direction)
    {
        this.owner = owner;
        moveDirection = direction; 
        Physics2D.IgnoreCollision(_collider2D, gameObject.GetComponent<Collider2D>());
    }

    public void SetBulletVelocity(Vector2 velocity)
    {
        rb2d.velocity = velocity * BulletMoveSpeed;
        SetBulletVelocityClientRpc(velocity);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void SetBulletVelocityClientRpc(Vector2 velocity)
    {
        if (IsHost)
        {
            return;
        }

        rb2d.velocity = velocity * BulletMoveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log($"Collision detected with {other.gameObject}");
        // register collision only on network
        if (!NetworkManager.Singleton.IsServer && !NetworkObject.IsSpawned) return;
        if (owner != other.gameObject)
        {
            HealthComponent otherObjectHealthComponent = other.gameObject.GetComponent<HealthComponent>();
            if (otherObjectHealthComponent)
            {
                otherObjectHealthComponent.TakeDamage(damage, NetworkObjectId);
                SelfDestroy();
            }
        }
    
    /*if(owner.TryGetComponent(out PlayersScore playerScore))
    {
        if(other.gameObject.TryGetComponent(out ObjectStats objectStats))
            playerScore.UpdateTheScore(objectStats.score);
    }*/
    }
    
    private void SelfDestroy()
    {
        if (!NetworkObject.IsSpawned)
        {
            return;
        }
        NetworkObject.Despawn(true);
    }
    
}
