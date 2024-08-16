using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button disconnectButton;

    private void Start()
    {
        if(disconnectButton!=null)
        disconnectButton.gameObject.SetActive(false);
    }

    public void StartHost()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        disconnectButton.gameObject.SetActive(true);

        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        disconnectButton.gameObject.SetActive(true);
        NetworkManager.Singleton.StartClient();
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void ShowNetworkButtons()
    {
        hostButton.gameObject.SetActive(true);
        clientButton.gameObject.SetActive(true);
    }

}

