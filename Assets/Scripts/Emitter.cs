using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Emitter : NetworkBehaviour
{
    [SerializeField] private GameObject ObjectToSpawn;
    [SerializeField] private int AmountToSpawn = 10;
    [SerializeField] private Collider2D collider;
    private int spawnCount = 1;
    private float SpawnInteraval = 1.5f;
    private float PreviousSpawnTime = 0f;
    private bool bSpawning = false;

    private void Start()
    {
        bSpawning = true;
    }

    private void Update()
    {
        if (!bSpawning) return;
            Spawning(Time.time);
    }

    public void Spawn(bool value)
    {
        bSpawning = value;
    }

    private void Spawning(float Time)
    {
        var currentTime = Time;
        if (currentTime - PreviousSpawnTime >= SpawnInteraval)
        {
            PreviousSpawnTime = currentTime;
            SpawnObjectRPC();
        }
        
    }
    
    [Rpc(SendTo.Server)]
    private void SpawnObjectRPC()
    {
        if (spawnCount != AmountToSpawn)
        {
            GameObject ob = ObjectToSpawn;
            Vector3 randomPoint = new Vector3(Random.Range(collider.bounds.min.x, collider.bounds.max.x), 0f, 0f);
            ob.GetComponent<Meteor>().Init(randomPoint);
            NetworkManager.Instantiate(ObjectToSpawn);
            
            spawnCount++;
        }
        else
        {
            bSpawning = false;
        }
    }
}
