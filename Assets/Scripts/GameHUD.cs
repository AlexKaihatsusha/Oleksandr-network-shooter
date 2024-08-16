using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : NetworkBehaviour
{
    public static GameHUD Singleton;
    [SerializeField] private Button _readyButton;

    private void Awake()
    {
        
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        _readyButton.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.OnClientConnectedCallback += ShowReadyButtonUIRPC;
        _readyButton.onClick.AddListener(HideReadyButton);
    }

    private void HideReadyButton()
    {
        StartGame();
        _readyButton.gameObject.SetActive(false);
    }
    public void StartGame()
    {
        GameManager.Singleton.StartGame();
    }
   
    [Rpc(SendTo.Server)]
    private void ShowReadyButtonUIRPC(ulong @ulong)
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= ShowReadyButtonUIRPC;
        
        Debug.Log($"OnClientConnected Callback: { NetworkManager.Singleton.ConnectedClients.Count} players on server");
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            Debug.Log("Show ready button");
            //show the button only for host
            if(IsHost)_readyButton.gameObject.SetActive(true);
        }
    }
    
}
