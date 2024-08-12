using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    NetworkVariable<Vector2> moveDirection = new NetworkVariable<Vector2>();
    
    [SerializeField] private GameObject bulletObject;
    [SerializeField] private GameObject Crosshair;
    
    private NetworkVariable<FixedString32Bytes> networkPlayerName = new NetworkVariable<FixedString32Bytes>("Player");
    
    [SerializeField] private TMP_Text playerName;
    private HealthComponent healthComponent;
    private Camera _mainCamera;
    private Vector3 _mouseInput;
    public delegate void OnDeath();

    public OnDeath delegateOnDeath;
    //private Vector2 moveDirection = Vector2.zero;
    public InputMap ActionMap;
    private void Start()
    {
        if(!IsLocalPlayer) return;
            ActionMap = new InputMap();
            ActionMap.PlayerInput.Movement.Enable();
            healthComponent = GetComponent<HealthComponent>();
            healthComponent.Init(this.gameObject);
            delegateOnDeath = SelfDestroy;
            _mainCamera = Camera.main;
    }
    
    private void Awake()
    {
        _mainCamera = Camera.main;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        networkPlayerName.Value = "Player " + (OwnerClientId + 1);
        playerName.text = networkPlayerName.Value.ToString();
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

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            if (context.performed)
            {
                Vector3 shootDirection = MousePositionInWorld() - transform.position;
                shootDirection.Normalize();
                shootDirection.z = 0f;
                float angleInRadians = Mathf.Atan2(shootDirection.x, shootDirection.y);
                float angleInDergees = Mathf.Rad2Deg * angleInRadians;
                Quaternion shootRotation = Quaternion.Euler(new Vector3(0f,0f,-angleInDergees));
                ShootServerRPC(shootDirection, shootRotation);   
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if(!Application.isFocused) return;
        
        transform.position += new Vector3(moveDirection.Value.x, moveDirection.Value.y, 0f) * (speed * Time.deltaTime);
        if (IsOwner)
        {
            UpdateCrosshairPositionAndRotation(MousePositionInWorld()); 
        }
    }

    public void UpdateCrosshairPositionAndRotation(Vector3 MousePosition)
    {
        Vector3 aim_direction = MousePosition - transform.position;
        aim_direction.Normalize();
       
        //Debug.Log($"direction {aim_direction}");
        Vector3 crosshair_position = transform.position + aim_direction * 0.5f;
        crosshair_position.z = 0f;
        //Debug.Log($"position {crosshair_position}");
        float angleInRadians = Mathf.Atan2(aim_direction.x, aim_direction.y);
        float angleInDergees = Mathf.Rad2Deg * angleInRadians;
        Crosshair.transform.rotation = Quaternion.Euler(new Vector3(0f,0f, -angleInDergees));
        Crosshair.transform.position = crosshair_position;
    }
    
    
    Vector3 MousePositionInWorld()
    {
        _mouseInput = Input.mousePosition;
        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(_mouseInput);
        mouseWorldPosition.z = 0f;
        //Debug.Log($"mouse position {mouseWorldPosition}");
        return mouseWorldPosition;
    }
    private void SpawnBullet(Vector3 ShootDirection, Quaternion ShootRotation)
    {
        //Debug.Log("Server spawns bullet");
        var bullet = Instantiate(bulletObject);
        var bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bullet.GetComponent<Bullet>().Init(transform.gameObject, transform.position, ShootRotation, ShootDirection);
        bulletNetworkObject.Spawn();
    }

   
    [ServerRpc]
    private void ShootServerRPC(Vector3 ShootDirection, Quaternion ShootRotation)
    {
        //Debug.Log("Server shoot RPC;");
        SpawnBullet(ShootDirection,ShootRotation);
    }
    
    [Rpc(SendTo.Server)]
    private void MoveRPC(Vector2 value)
    {
        //Debug.Log("2 - MoveRPC call");
        moveDirection.Value = value;
    }
    
    private void SelfDestroy()
    {
        if(NetworkObject.IsSpawned)
        {
            //NetworkObject.Despawn(this);
        }
        Destroy(this);
    }

}
