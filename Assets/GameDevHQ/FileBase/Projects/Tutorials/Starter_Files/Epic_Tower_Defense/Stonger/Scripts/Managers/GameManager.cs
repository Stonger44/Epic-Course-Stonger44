﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField] private int _initialWaveEnemyCount;
    [SerializeField] private int _initialHealth;
    [SerializeField] private int _health;
    [SerializeField] private GameObject _healthBarGameObject;

    public float HealthCautionThreshold { get; private set; }
    public float HealthWarningThreshold { get; private set; }
    [SerializeField] private float _healthCautionThreshold;
    [SerializeField] private float _healthWarningThreshold;
    private float _healthPercent;

    public int TotalWarFunds { get; private set; }
    [SerializeField] private int _totalWarFunds;

    public int CurrentWaveInitialTotalWarFunds { get; private set; }
    [SerializeField] private int _currentWaveInitialTotalWarFunds;

    public int InitialWave { get; private set; }
    public int Wave { get; private set; }
    public int FinalWave { get; private set; }
    [SerializeField] private int _initialWave = 1;
    [SerializeField] private int _wave;
    [SerializeField] private int _finalWave;

    public int CurrentWaveTotalEnemyCount { get; private set; }
    [SerializeField] private int _currentWaveTotalEnemyCount;
    [SerializeField] private int _currentWaveCurrentEnemyCount;

    public bool WaveRunning { get; private set; }
    public bool WaveSuccess { get; private set; }

    private AudioSource[] _totalAudioSourceArray;

    private bool _isStartWaveRoutineRunning = false;

    public static event Action onStartWave;
    public static event Action<GameObject> onGainedWarFundsFromEnemyDeath;
    public static event Action<GameObject, float> onHealthUpdate;
    public static event Action<int, int> onTakeDamage;
    public static event Action<int, int> onWaveUpdate;
    public static event Action<int, int> onEnemyCountUpdate;

    public static event Action onUpdateLevelStatus;
    public static event Action<int> onUpdateLevelStatusCountDown;

    public static event Action<int> onWaveFailed;
    public static event Action onSelfDestructTowers;

    public static event Action onCollectCurrentActiveTowersTotalWarFundValue;

    private WaitForSeconds _waitForSeconds_CountDownPrep = new WaitForSeconds(0.44f);
    private WaitForSeconds _waitForSeconds_CountDown = new WaitForSeconds(1);

    [SerializeField] private AudioSource _backgroundMusic;

    public override void Init()
    {
        Time.timeScale = 1;

        HealthCautionThreshold = _healthCautionThreshold;
        HealthWarningThreshold = _healthWarningThreshold;

        InitialWave = _initialWave;
        Wave = InitialWave;
        _wave = Wave;
        FinalWave = _finalWave;
        onWaveUpdate?.Invoke(_wave, _finalWave);
        ResetWaveEnemyCount();
    }

    private void OnEnable()
    {
        //Subscribe to events
        EndPoint.onEndPointReached += TakeDamage;
        Enemy.onDeath += OnEnemyExplosion;
        Enemy.onResetComplete += OnEnemyResetComplete;
        TowerLocation.onPurchaseTower += SpendWarFunds;
        TowerLocation.onDismantledCurrentTower += CollectDismantledTowerWarFunds;
        TowerLocation.onRepairedCurrentTower += SpendWarFunds;
        TowerLocation.onPurchaseTowerUpgrade += SpendWarFunds;
        Gatling_Gun.onBroadcastTowerWarFundValue += SaveCurrentWaveInitialTotalWarFunds;
        Missile_Launcher.onBroadcastTowerWarFundValue += SaveCurrentWaveInitialTotalWarFunds;
    }

    private void OnDisable()
    {
        //Unsubscribe from events
        EndPoint.onEndPointReached -= TakeDamage;
        Enemy.onDeath -= OnEnemyExplosion;
        Enemy.onResetComplete -= OnEnemyResetComplete;
        TowerLocation.onPurchaseTower -= SpendWarFunds;
        TowerLocation.onDismantledCurrentTower -= CollectDismantledTowerWarFunds;
        TowerLocation.onRepairedCurrentTower -= SpendWarFunds;
        TowerLocation.onPurchaseTowerUpgrade -= SpendWarFunds;
        Gatling_Gun.onBroadcastTowerWarFundValue -= SaveCurrentWaveInitialTotalWarFunds;
        Missile_Launcher.onBroadcastTowerWarFundValue -= SaveCurrentWaveInitialTotalWarFunds;
    }

    private void Start()
    {
        WaveRunning = false;
        WaveSuccess = true;

        _health = _initialHealth;
        _healthPercent = (float)_health / (float)_initialHealth;
        onHealthUpdate?.Invoke(_healthBarGameObject, _healthPercent);

        TotalWarFunds = _totalWarFunds;
        UI_Manager.Instance.UpdateWarFundsText(TotalWarFunds);
    }

    private void Update()
    {

    }
    
    public void OnPlaybackButtonPressed(float timescale)
    {
        _totalAudioSourceArray = FindObjectsOfType<AudioSource>();

        Time.timeScale = timescale;

        if (Time.timeScale == 0)
        {
            foreach (var audioSource in _totalAudioSourceArray)
                audioSource.Pause();

            _backgroundMusic.Pause();
        }
        else
        {
            foreach (var audioSource in _totalAudioSourceArray)
                audioSource.UnPause();

            _backgroundMusic.UnPause();

            if (WaveRunning == false)
                StartWave();
        }
    }

    public void OnRestartButtonPress()
    {
        Time.timeScale = 1;
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(0.05f);
        SceneManager.LoadScene(0);
    }

    private void StartWave()
    {
        if (_isStartWaveRoutineRunning == false)
        {
            _isStartWaveRoutineRunning = true;

            if (WaveSuccess == false) //The previous wave was a fail, so this is a retry
            {
                TotalWarFunds = CurrentWaveInitialTotalWarFunds;
                _totalWarFunds = TotalWarFunds;

                UI_Manager.Instance.UpdateWarFundsText(TotalWarFunds);
            }

            CurrentWaveInitialTotalWarFunds = 0;
            _currentWaveInitialTotalWarFunds = 0;

            onCollectCurrentActiveTowersTotalWarFundValue?.Invoke();
            SaveCurrentWaveInitialTotalWarFunds(TotalWarFunds);
            
            StartCoroutine(StartWaveRoutine()); 
        }
    }

    private void SaveCurrentWaveInitialTotalWarFunds(int warFundsToAddToTotal)
    {
        CurrentWaveInitialTotalWarFunds += warFundsToAddToTotal;
        _currentWaveInitialTotalWarFunds = CurrentWaveInitialTotalWarFunds;
    }

    private IEnumerator StartWaveRoutine()
    {
        //CountDown
        for (int i = 4; i > -1; i--)
        {
            onUpdateLevelStatusCountDown?.Invoke(i);
            if (i == 4)
                yield return _waitForSeconds_CountDownPrep;
            else
                yield return _waitForSeconds_CountDown;
        }
        onUpdateLevelStatusCountDown?.Invoke(-1); //The LevelStatusUI will hide itself with this parameter

        //Prepare for new wave
        ResetWaveEnemyCount();

        if (_health <= 0)
            _health = _initialHealth;
        _healthPercent = (float)_health / (float)_initialHealth;
        onHealthUpdate?.Invoke(_healthBarGameObject, _healthPercent);

        //Being Wave
        WaveRunning = true;
        WaveSuccess = false;

        _wave = Wave;

        onWaveUpdate?.Invoke(_wave, _finalWave);
        onStartWave?.Invoke();

        _backgroundMusic.loop = true;
        _backgroundMusic.time = 0f;
        _backgroundMusic.Play();

        _isStartWaveRoutineRunning = false;
    }

    private void WaveComplete()
    {
        WaveRunning = false;
        WaveSuccess = true;
        Wave++;

        onUpdateLevelStatus?.Invoke();
    }

    private void WaveFailed()
    {
        WaveRunning = false;
        WaveSuccess = false;

        onUpdateLevelStatus?.Invoke();
        onWaveFailed?.Invoke(_health);
        onSelfDestructTowers?.Invoke();
        _backgroundMusic.loop = false;
        _backgroundMusic.time = 120;
    }

    private void TakeDamage()
    {
        if (WaveRunning == true)
        {
            _health--;

            if (_health <= 0)
            {
                _health = 0;
                _healthPercent = (float)_health / (float)_initialHealth;
                onHealthUpdate?.Invoke(_healthBarGameObject, _healthPercent);
                onTakeDamage?.Invoke(_health, _initialHealth);
                WaveFailed();
                return;
            }
            _healthPercent = (float)_health / (float)_initialHealth;
            onHealthUpdate?.Invoke(_healthBarGameObject, _healthPercent);
            onTakeDamage?.Invoke(_health, _initialHealth); 
        }
    }

    private void OnEnemyExplosion(GameObject enemy)
    {
        var enemyScript = enemy.GetComponent<Enemy>();

        if (enemyScript == null)
            Debug.Log("enemyScript is NULL.");

        TotalWarFunds += enemyScript.warFunds;
        _totalWarFunds = TotalWarFunds;
        UI_Manager.Instance.UpdateWarFundsText(TotalWarFunds);

        if (TowerManager.Instance.IsViewingTower == true)
            onGainedWarFundsFromEnemyDeath?.Invoke(TowerManager.Instance.CurrentlyViewedTower);

        _currentWaveCurrentEnemyCount--;
        onEnemyCountUpdate.Invoke(_currentWaveCurrentEnemyCount, _currentWaveTotalEnemyCount);
    }

    private void OnEnemyResetComplete(GameObject enemy)
    {
        if (Enemy.enemyCount <= 0)
        {
            _backgroundMusic.loop = false;
            _backgroundMusic.time = 120;
            WaveComplete();
        }
    }

    private void ResetWaveEnemyCount()
    {
        CurrentWaveTotalEnemyCount = _initialWaveEnemyCount * Wave;
        _currentWaveTotalEnemyCount = CurrentWaveTotalEnemyCount;
        _currentWaveCurrentEnemyCount = CurrentWaveTotalEnemyCount;
        onEnemyCountUpdate.Invoke(_currentWaveCurrentEnemyCount, _currentWaveTotalEnemyCount);
    }

    private void SpendWarFunds(int warFundsSpent)
    {
        TotalWarFunds -= warFundsSpent;
        _totalWarFunds = TotalWarFunds;
        UI_Manager.Instance.UpdateWarFundsText(TotalWarFunds);
    }

    private void CollectDismantledTowerWarFunds(int warFundsAcquired)
    {
        TotalWarFunds += warFundsAcquired;
        _totalWarFunds = TotalWarFunds;
        UI_Manager.Instance.UpdateWarFundsText(TotalWarFunds);
    }

    public int GetCurrentWaveCurrentEnemyCount() => _currentWaveCurrentEnemyCount;
}
