using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Meteor : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int damageAmount = 20;

    public void Init(Vector3 position)
    {
        transform.position = position;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        var otherGameObjectHealthComponent = other.gameObject.GetComponent<HealthComponent>();
        
        if (otherGameObjectHealthComponent)
        {
            otherGameObjectHealthComponent.TakeDamage(damageAmount, this.gameObject);
            SelfDestroy();
        }
        else
        {
            SelfDestroy();
        }
    }
    
    private void SelfDestroy()
    {
        //Debug.Log("Destroy meteor");
        if(NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(this.gameObject);
            
        }   
        Destroy(this.gameObject);
    }

    void Update()
    {
        //since we spawn it from Emitter and Emitter is Server authority
        if (!IsServer) return;
            transform.position += new Vector3(0f, -1f, 0f) * (moveSpeed * Time.deltaTime);
    }
}
