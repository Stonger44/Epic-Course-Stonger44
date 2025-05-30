﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRange : MonoBehaviour
{
    [SerializeField] private GameObject _towerRangeGreen;
    [SerializeField] private GameObject _towerRangeRed;

    private void OnEnable()
    {
        TowerManager.onBrowsingTowerLocations += ToggleTowerRange;
        TowerLocation.onLocationMouseOver += ToggleTowerColor;
        TowerLocation.onLocationMouseExit += ShowTowerRange_Red;
        TowerLocation.onInsufficientWarFunds += ShowTowerRange_Red;
    }

    private void OnDisable()
    {
        TowerManager.onBrowsingTowerLocations -= ToggleTowerRange;
        TowerLocation.onLocationMouseOver -= ToggleTowerColor;
        TowerLocation.onLocationMouseExit -= ShowTowerRange_Red;
        TowerLocation.onInsufficientWarFunds -= ShowTowerRange_Red;
    }

    private void ToggleTowerRange(bool isPlacingTower)
    {
        if (isPlacingTower)
        {
            ShowTowerRange_Red();
        }
        else
        {
            HideTowerRange();
        }
    }

    private void ToggleTowerColor(bool isGood)
    {
        if (isGood)
            ShowTowerRange_Green();
        else
            ShowTowerRange_Red();
    }

    private void HideTowerRange()
    {
        _towerRangeGreen.SetActive(false);
        _towerRangeRed.SetActive(false);
    }

    private void ShowTowerRange_Green()
    {
        _towerRangeGreen.SetActive(true);
        _towerRangeRed.SetActive(false);
    }

    private void ShowTowerRange_Red()
    {
        _towerRangeRed.SetActive(true);
        _towerRangeGreen.SetActive(false);
    }
}
