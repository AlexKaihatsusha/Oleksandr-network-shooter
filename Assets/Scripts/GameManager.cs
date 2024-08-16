using System;
using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.Infrastructure;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;

public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton;

    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text currentWaveText;
    [SerializeField] private TMP_Text scoreText;

    [SerializeField] private int wavesAmount = 5;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float waveDuration = 10.0f;
    
    public NetworkVariable<int> score = new NetworkVariable<int>(0);
    private NetworkVariable<float> countdown = new NetworkVariable<float>();
    private NetworkVariable<int> currentWave = new NetworkVariable<int>(0);
    private NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);  
    private bool isWaveActive = false;
 
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
        ShowGameDataRPC();
        enabled = IsServer;
        if (IsServer)
        {
            countdown.Value = timeBetweenWaves;
            currentWave.Value = 0;
            isWaveActive = false;
            gameStarted.Value = false;
        }
        countdown.OnValueChanged += UpdateCountDownUI;
        currentWave.OnValueChanged += UpdateWaveCountUI;
        score.OnValueChanged += UpdateScoreUI;
    }

    [Rpc(SendTo.Everyone)]
    private void ShowGameDataRPC()
    {
        countdownText.text = "-";
        currentWaveText.text = "Wave: 0";
        scoreText.text = "Score: 0";
    }
    
   
    [Rpc(SendTo.Server)]
    public void UpdateTheScoreRpc(int score)
    {
        this.score.Value += score;
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
        StartCoroutine
        (
            HandleWaveDuration
            (
            waveDuration, () =>
            {
                ToggleEmitter(false);
                isWaveActive = false;
                
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
    private void UpdateScoreUI(int previousValue, int newValue)
    {
        if (scoreText !=  null)
        {
            scoreText.text = "Score: " + newValue;
        }
    }

}
