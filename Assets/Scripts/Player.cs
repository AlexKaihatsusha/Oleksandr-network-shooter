using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    NetworkVariable<Vector2> moveDirection = new NetworkVariable<Vector2>();
    //private Vector2 moveDirection = Vector2.zero;
    public InputMap ActionMap;
    private void Start()
    {
        if(!IsLocalPlayer) return;
            ActionMap = new InputMap();
            ActionMap.PlayerInput.Movement.Enable();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(!IsServer) return;
        transform.position += new Vector3(moveDirection.Value.x, moveDirection.Value.y, 0f) * (speed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
       if(IsOwner)
       {
            Vector2 direction = context.ReadValue<Vector2>();
            MoveRPC(direction);
            //Debug.Log("1 - OnMove call");
       }
    }
    [Rpc(SendTo.Server)]
    private void MoveRPC(Vector2 value)
    {
        //Debug.Log("2 - MoveRPC call");
        moveDirection.Value = value;
    }
}
