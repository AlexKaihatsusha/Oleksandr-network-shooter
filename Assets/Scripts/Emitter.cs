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

    private void Awake()
    {
        collider = GetComponent<Collider2D>();
    }

  

    public override void OnNetworkSpawn()
    {
        // Only the server spawns, clients will disable this component on their side
        enabled = IsServer;
        if (!enabled || ObjectToSpawn == null)
        {
            return;
        }
        bSpawning = false;
    }

    private void Update()
    {
        if (!bSpawning && !IsServer) return;
            Spawning(Time.time);
    }
    
    //to set value if want to start spawning in specific time in game (example: GameManager starts the game after countdown, then set a value to true, spawning starts)
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
            SpawnObjectServerRPC();
        }
    }
    
    [ServerRpc]
    private void SpawnObjectServerRPC()
    {
        
        if (spawnCount != AmountToSpawn && bSpawning)
        {
            //generate random point on Emitter collider
            Vector3 randomPoint = new Vector3(Random.Range(collider.bounds.min.x, collider.bounds.max.x), transform.position.y, 0f);
            //Instantiate our prefab
            var instance = Instantiate(ObjectToSpawn);
            //Get Network object
            var instanceNetworkObject = instance.GetComponent<NetworkObject>();
            //Use class init function, to set position
            instance.GetComponent<Meteor>().Init(randomPoint);
            //Spawn on network
            instanceNetworkObject.Spawn();
            spawnCount++;
            
        }
        else
        {
            bSpawning = false;
        }
    }
}
