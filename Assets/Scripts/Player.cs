using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.BossRoom.Infrastructure;
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
    
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject CrosshairGameObject;
    
    private NetworkVariable<FixedString32Bytes> networkPlayerName = new NetworkVariable<FixedString32Bytes>("Player");
    
    [SerializeField] private TMP_Text playerName;
    private HealthComponent healthComponent;
    private Rigidbody2D rb2d;
    private Camera _mainCamera;
    private Vector3 _mouseInput;
    [SerializeField] private Chat _chatRef;
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
        _chatRef = FindObjectOfType<Chat>();
        _mainCamera = Camera.main;
        if (rb2d == null)
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        rb2d.isKinematic = false;
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

    public void OnMessageSend(InputAction.CallbackContext context)
    {
        
        try
        {
            if (IsOwner)
            {
                if (context.performed)
                {

                    if (_chatRef != null)
                    {
                        _chatRef.OnSend();
                    }
                    else
                    {
                        Debug.Log("Chat component not found in scene");
                    }
                }
            }
        }
        catch
        {
            Debug.LogError("Chat component is NULL");
        }
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            if (context.performed)
            {
                //calculate shoot direction
                Vector3 shootDirection = MousePositionInWorld() - transform.position;
                shootDirection.Normalize();
                shootDirection.z = 0f;
                //calculate angle to rotate bullet object
                float angleInRadians = Mathf.Atan2(shootDirection.x, shootDirection.y);
                float angleInDergees = Mathf.Rad2Deg * angleInRadians;
                //set to variable only Z rotation(since it's 2D)
                Quaternion shootRotation = Quaternion.Euler(new Vector3(0f,0f,-angleInDergees));
                //send to Server with parameters
                ShootServerRPC(shootDirection, shootRotation);   
                
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            rb2d.velocity = moveDirection.Value * speed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!Application.isFocused) return;
        
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
        CrosshairGameObject.transform.rotation = Quaternion.Euler(new Vector3(0f,0f, -angleInDergees));
        CrosshairGameObject.transform.position = crosshair_position;
    }
    
    
    Vector3 MousePositionInWorld()
    {
        _mouseInput = Input.mousePosition;
        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(_mouseInput);
        mouseWorldPosition.z = 0f;
        //Debug.Log($"mouse position {mouseWorldPosition}");
        return mouseWorldPosition;
    }

    [Rpc(SendTo.Server)]
    private void ShootServerRPC(Vector3 ShootDirection, Quaternion ShootRotation)
    {
        SpawnBullet(ShootDirection, ShootRotation);
    }


    private void SpawnBullet(Vector3 shootDirection, Quaternion shootRotation)
    {
        GameObject bullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, transform.position + shootDirection * 1f, shootRotation).gameObject;
        if (bullet == null)
        {
            Debug.LogWarning("Failed to get gameobject from pool");
        }
        NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        if(!bulletNetworkObject.IsSpawned)
        {
            bulletNetworkObject.Spawn();
        }
        bullet.GetComponent<Bullet>().Init(gameObject, shootDirection);
        bullet.GetComponent<Bullet>().SetBulletVelocity(shootDirection);
        
    }
    
    [Rpc(SendTo.Server)]
    private void MoveRPC(Vector2 value)
    {
        //Debug.Log("2 - MoveRPC call");
        moveDirection.Value = value;
    }
    
    private void SelfDestroy()
    {
        /*if(NetworkObject.IsSpawned)
        {
            //NetworkObject.Despawn(this);
        }
        Destroy(this);*/
    }

}
