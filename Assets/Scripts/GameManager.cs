using System;
using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.Infrastructure;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton;

    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text currentWaveText;

    [SerializeField] private int wavesAmount = 5;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float waveDuration = 10.0f;

    private NetworkVariable<float> countdown = new NetworkVariable<float>();
    private NetworkVariable<int> currentWave = new NetworkVariable<int>(0);
    private NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);
    private bool isWaveActive = false;
    [SerializeField] private GameObject chatGameObject;
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
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            countdown.Value = timeBetweenWaves;
            currentWave.Value = 0;
            isWaveActive = false;
            gameStarted.Value = false;
            countdownText.text = "";
            currentWaveText.text = "";
        }
        countdown.OnValueChanged += UpdateCountDownUI;
        currentWave.OnValueChanged += UpdateWaveCountUI;
    }
    
    private void Update()
    {
        if (!IsServer || !gameStarted.Value) return;
        
        if (!isWaveActive)
        {
            countdown.Value -= Time.deltaTime;
            if (countdown.Value <= 0)
            {
                if (currentWave.Value < wavesAmount)
                {
                    StartWave();
                }
                else
                {
                    HandleGameEnd();
                }
            }
        }
    }

    private void StartWave()
    {
        currentWave.Value++;
        isWaveActive = true;
        ToggleEmitter(true);
        if (chatGameObject != null)
        {
            chatGameObject.SetActive(false);
        }
        StartCoroutine
        (
            HandleWaveDuration
            (
            waveDuration, () =>
            {
                ToggleEmitter(false);
                isWaveActive = false;
                if (chatGameObject != null)
                {
                    chatGameObject.SetActive(true);
                }
                if (currentWave.Value <= wavesAmount)
                {
                    countdown.Value = timeBetweenWaves;
                    isWaveActive = false;
                }
            }
            )
        );

    }

    private IEnumerator HandleWaveDuration(float waveDuration, Action stopWaveAction)
    {
        yield return new WaitForSeconds(waveDuration);
        stopWaveAction.Invoke();
    }
    private void ToggleEmitter(bool spawn)
    {
        GameObject emitterGameObject = GameObject.Find("Emitter");
        if (emitterGameObject != null && emitterGameObject.TryGetComponent<Emitter>(out Emitter emitter))
        {
            emitter.Spawn(spawn);
        }
    }


    public void StartGame()
    {
        if (!gameStarted.Value)
        {
            gameStarted.Value = true;
        }
    }

    private void HandleGameEnd()
    {
        Debug.Log("GAME END");
        ToggleEmitter(false);
        countdown.Value = 0;
        currentWave.Value = 0;
    }
    private void UpdateCountDownUI(float previousValue, float newValue)
    {
        if (countdownText != null)
        {
            if (newValue <= 0.001f)
            {
                countdownText.text = "";
            }
            else
            {
                countdownText.text = isWaveActive ? "Wave in progress" :  Mathf.CeilToInt(countdown.Value) + "sec";
            }
        }
    }

    private void UpdateWaveCountUI(int previousValue, int newValue)
    {
        if (currentWaveText != null)
        {
            currentWaveText.text = "Wave: " + newValue;
        }
    }

}
