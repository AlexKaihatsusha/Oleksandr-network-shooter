using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton;
    [SerializeField] private int WavesAmount = 2;
    
    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = new GameManager();
        }

        //NetworkManager.Singleton.OnClientConnectedCallback += UpdatePlayerName;
    }

    
    
    public void StartGame()
    {
        GameObject gameObject = GameObject.Find("Emitter");
        if (gameObject != null && gameObject.TryGetComponent<Emitter>(out Emitter emitter))
        {
            emitter.Spawn(true);
        }
    }

}
