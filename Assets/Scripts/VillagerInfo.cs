using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerInfo : MonoBehaviour
{
    public int seeds;
    public int crops;
    public OverlayTile activeTile;
    private OverlayTile _prevActiveTile;
    public bool plantingState;
    public bool harvestingState;
    public float speed;
    public float rangeWaitTime;
    // Start is called before the first frame update
    void Start()
    {
        seeds = 0;
        crops = 0;
        seeds = MapManager.Instance.map.Count;
    }

    private void Update()
    {
        if (_prevActiveTile != activeTile)
        {
            Debug.Log("Villager's active tile has changed!");
            _prevActiveTile = activeTile;

            if (plantingState)
            {
                PerformPlantSteps();
            }

            if (harvestingState)
            {
                PerformHarvestSteps();
            }
        }
    }

    private void PerformHarvestSteps()
    {
        if (_prevActiveTile.isTilled)
        {
            AudioManager.Instance.PlayRandomPlant();
        }
        if (!_prevActiveTile.isTilled)
        {
            AudioManager.Instance.PlayRandomStep();
        }
    }

    private void PerformPlantSteps()
    {
        if (_prevActiveTile.isTilled && _prevActiveTile.hasSeed)
        {
            Debug.Log("here");
            AudioManager.Instance.PlayRandomPlant();
        }
        if (!_prevActiveTile.isTilled)
        {
            AudioManager.Instance.PlayRandomStep();
        }
    }
}
