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
    [SerializeField] private TMP_Text _playersScore;
    [SerializeField] private Button _readyButton;
    [SerializeField] private TMP_Text _timer;

    private void Awake()
    {
        
        if (Singleton == null)
        {
            Singleton = new GameHUD();
        }
        _readyButton.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.OnClientConnectedCallback += ShowReadyButtonUI;
        _readyButton.onClick.AddListener(HideReadyButton);
    }

    private void HideReadyButton()
    {
        _readyButton.gameObject.SetActive(false);
    }
    public void StartGame()
    {
        GameManager.Singleton.StartGame();
    }
    public void UpdateScores()
    {
        
    }
   
    //[Rpc(SendTo.Server)]
    private void ShowReadyButtonUI(ulong @ulong)
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= ShowReadyButtonUI;
        
        Debug.Log($"OnClientConnected Callback: { NetworkManager.Singleton.ConnectedClients.Count} players on server");
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            Debug.Log("Show ready button");
            //show the button only for host
            if(IsHost)_readyButton.gameObject.SetActive(true);
            

        }
    }
    private void Update()
    {
       
    }
}
