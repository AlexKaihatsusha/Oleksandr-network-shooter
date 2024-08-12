using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    private GameObject owner;
    
    [SerializeField] private float LifeSpan = 2f;
    private float SpawnTime = 0f;
    
    [SerializeField] private float BulletMoveSpeed = 3f;
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Vector3 direction;
    public void Init(GameObject owner, Vector3 position, Quaternion rotation, Vector3 direction)
    {
        this.owner = owner;
        this.transform.position = position + direction * 0.85f;
        this.transform.rotation = rotation;
        this.direction = direction;
        //Physics2D.IgnoreCollision(_collider2D,owner.GetComponent<Collider2D>());
       
        //Debug.Log($"{this.owner.ToString()}is set");
    }

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        SpawnTime = Time.time;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log($"Collision detected with {other.gameObject}");
       
        HealthComponent otherObjectHealthComponent = other.gameObject.GetComponent<HealthComponent>();
        if (otherObjectHealthComponent)
        {
            otherObjectHealthComponent.TakeDamage(damage, owner);
            SelfDestroy();
        }
        if(owner.TryGetComponent(out PlayersScore playerScore))
        {
            if(other.gameObject.TryGetComponent(out ObjectStats objectStats))
                playerScore.UpdateTheScore(objectStats.score);
        }
    }

  
    private void CheckLifeTime(float Time)
    {
        var currentTime = Time;
        if (currentTime - SpawnTime >= LifeSpan)
        {
            SelfDestroy();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(!IsServer) return;
            CheckLifeTime(Time.time);
            transform.position += direction * (BulletMoveSpeed * Time.deltaTime);
        
    }
    private void SelfDestroy()
    {
        //Debug.Log("Destroy bullet");
        if(NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(this.gameObject);
            
        }   
        Destroy(this.gameObject);
    }
    
}
