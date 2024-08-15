using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Chat : NetworkBehaviour
{
    [SerializeField] private GameObject chatMessagePrefab;
    [SerializeField] private Transform chatContent;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private int maxMessageLength = 125;
    private FixedString128Bytes messageToSend = "";

    [SerializeField] private GameObject chatGameObject;

    private void Start()
    {
        chatGameObject.SetActive(true);
    }
    

    public void OnSend()
    {
        Debug.Log("Send message call");
        string message = _inputField.text;
        if (message.Length == 0)
        {
            return;
        }
        if (message.Length > maxMessageLength)
        {
            message = message.Substring(0, maxMessageLength);
        }
        Debug.Log($"message: {message}");
        FixedString128Bytes messageToSend = new FixedString128Bytes(message);
        _inputField.text = "";
        SubmitMessageRPC(messageToSend);
    }
    [Rpc(SendTo.Server)]
    public void SubmitMessageRPC(FixedString128Bytes message)
    {
        UpdateMessageRPC(message);
        Debug.Log("Message submit RPC call");
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateMessageRPC(FixedString128Bytes message)
    {
        //Debug.Log("Message update RPC call");
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        TextMeshProUGUI messageText = newMessage.GetComponent<TextMeshProUGUI>();
        messageText.text = message.ToString();
        ScrollRect scrollRect = chatContent.GetComponentInParent<ScrollRect>();
        scrollRect.normalizedPosition = new Vector2(0f,1f);
        //Canvas.ForceUpdateCanvases();
        //Debug.Log("Message Received");
    }
}
