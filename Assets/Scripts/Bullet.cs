using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    private GameObject owner;
    [SerializeField] private float LifeSpan = 2f;
    [SerializeField] private float BulletMoveSpeed = 3f;
    private float SpawnTime = 0f;
    // Start is called before the first frame update

    public void Init(GameObject owner, Transform transform)
    {
        this.owner = owner;
        this.transform.position = transform.position;
        Debug.Log($"{this.owner.ToString()}is set");
    }
    void Start()
    {
        
        SpawnTime = Time.time;
    }

    private void OnCollisionEnter(Collision other)
    {
        
        var otherGameObject = other.transform.gameObject;
        if (otherGameObject != owner)
        {
            HealthComponent otherObjectHealthComponent = otherGameObject.GetComponent<HealthComponent>();
            if (otherObjectHealthComponent)
            {
                otherObjectHealthComponent.TakeDamage(damage, owner);
                Destroy(this);
            }
        }
    }

    private void CheckLifeTime(float Time)
    {
        var currentTime = Time;
        if (currentTime - SpawnTime >= LifeSpan)
        {
            Debug.Log("Destroy bullet");
            if(NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(this);
            }
            Destroy(this.gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        CheckLifeTime(Time.time);
        transform.position += new Vector3(0, 1, 0f) * (BulletMoveSpeed * Time.deltaTime);
        
    }
}
