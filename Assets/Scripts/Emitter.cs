using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.BossRoom.Infrastructure;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Emitter : NetworkBehaviour
{
    [SerializeField] private GameObject PrefabToSpawn;
    private int spawnCount = 1;
    [SerializeField]private float SpawnInteraval = 1.5f;
    private float PreviousSpawnTime = 0f;
    private NetworkVariable<bool> bSpawning = new NetworkVariable<bool>(false);
    private List<Vector3> worldCorners = new List<Vector3>();
    private List<Vector3[]> worldEdges = new List<Vector3[]>();
    private Camera _mainCamera;
    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            worldCorners.Add(_mainCamera.ViewportToWorldPoint(new Vector3(0f,0f,0f)));//bottom left
            worldCorners.Add(_mainCamera.ViewportToWorldPoint(new Vector3(1f,0f,0f)));//bottom right
            worldCorners.Add(_mainCamera.ViewportToWorldPoint(new Vector3(0f,1f,0f)));//top left
            worldCorners.Add(_mainCamera.ViewportToWorldPoint(new Vector3(1f,1f,0f)));//top right
            Vector3[] bottomEdge = { worldCorners[0], worldCorners[1] };
            Vector3[] rightEdge = { worldCorners[1], worldCorners[3] };
            Vector3[] leftEdge = { worldCorners[0], worldCorners[2] };
            Vector3[] topEdge = { worldCorners[2], worldCorners[3] };
            worldEdges.Add(bottomEdge);
            worldEdges.Add(rightEdge);
            worldEdges.Add(leftEdge);
            worldEdges.Add(topEdge);
        }
    }

    private void Start()
    {
    }


    public override void OnNetworkSpawn()
    {
        // Only the server spawns, clients will disable this component on their side
        enabled = IsServer;
        if (!enabled || PrefabToSpawn == null)
        {
            return;
        }
    }

    private void Update()
    {
        if (!bSpawning.Value && !IsServer) return;
            Spawning(Time.time);
    }
    
    //to set value if want to start spawning in specific time in game (example: GameManager starts the game after countdown, then set a value to true, spawning starts)
    public void Spawn(bool value)
    {
        bSpawning.Value = value;
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
        if (bSpawning.Value)
        {
            //generate random point
            Vector3[] randomEdge = worldEdges[Random.Range(0, worldEdges.Count)];
            Vector3 randomPointOnEdge = Vector3.Lerp(randomEdge[0], randomEdge[1], Random.Range(0f, 1f));
            randomPointOnEdge.z = 0f;
            // move to center of world
            var direction = new Vector3(0f, 0f, 0f) - randomPointOnEdge;
            direction.Normalize();
            direction.z = 0f;
            //Instantiate our prefab
            Debug.Log("spawn meteor");
            NetworkObject meteorNetworkObject =
                NetworkObjectPool.Singleton.GetNetworkObject(PrefabToSpawn, randomPointOnEdge, quaternion.identity);
            meteorNetworkObject.GetComponent<Meteor>().Init(direction);
            meteorNetworkObject.GetComponent<Meteor>().prefab = PrefabToSpawn;
            if(!meteorNetworkObject.IsSpawned) 
                meteorNetworkObject.Spawn(true);
            //Use class init function, to set position
            spawnCount++;
        }
    }
}
