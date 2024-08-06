using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private float speed = 5f;
    private Vector2 moveDirection = Vector2.zero;
    public InputMap ActionMap;
    private InputAction move;
    private InputAction fire;
    
    private void Awake()
    {
        ActionMap = new InputMap();
    }
    
    private void OnEnable()
    {
        move = ActionMap.PlayerInput.Movement;
        move.Enable();
    }
    private void OnDisable()
    {
        move.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        rigidbody2D.velocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = move.ReadValue<Vector2>();
    }
}
