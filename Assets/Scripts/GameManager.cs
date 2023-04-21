using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState state;
    public static event Action<GameState> OnStateChange;

    [SerializeField] private MouseController mouseController;
    [SerializeField] private PlantManager plantManager;
    [SerializeField] private HarvestManager harvestManager;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject finish;
    [SerializeField] private GameObject instructions;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateGameState(GameState.SetUp);
        AudioManager.Instance.PlayRestart();
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.SetUp:
                HandleSetUp();
                break;
            case GameState.MouseControl:
                HandleMouseControl();
                break;
            case GameState.PlantSeeds:
                HandlePlantSeeds();
                break;
            case GameState.HarvestSeeds:
                HandleHarvestSeeds();
                break;
            case GameState.End:
                HandleEnd();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
        }

        OnStateChange?.Invoke(newState);

    }

    private void HandleEnd()
    {
        Debug.Log("GM: End");
        AudioManager.Instance.PlayEnd();
        finish.gameObject.SetActive(true);
        StartCoroutine(GoEnd());
    }
    IEnumerator GoEnd()
    {
        yield return new WaitForSeconds(3f);
        Loader.Load(Loader.Scene.End_Scene);
    }

    private void HandleSetUp()
    {
        Debug.Log("GM: Setting Up");
    }

    private void HandleMouseControl()
    {
        Debug.Log("GM: Mouse Control");
        mouseController.gameObject.SetActive(true);
    }

    private void HandlePlantSeeds()
    {
        Debug.Log("GM: Planting Seeds");
        mouseController.gameObject.SetActive(false);
        instructions.gameObject.SetActive(false);
        buttons.SetActive(false);
        plantManager.gameObject.SetActive(true);

    }

    private void HandleHarvestSeeds()
    {
        Debug.Log("GM: Harvesting Seeds");
        harvestManager.gameObject.SetActive(true);
        plantManager.gameObject.SetActive(false);
    }

    private void HandleGameOver()
    {
        Debug.Log("GM: Game Over");
        harvestManager.gameObject.SetActive(false);
        plantManager.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(true);
        harvestManager.gameObject.SetActive(false);
        plantManager.gameObject.SetActive(false);
    }

    public enum GameState
    {
        SetUp,
        MouseControl,
        PlantSeeds,
        HarvestSeeds,
        End,
        GameOver,
    }
}
