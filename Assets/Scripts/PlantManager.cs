using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public MouseController mouseController;
    private VillagerInfo villager;
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private Coroutine coroutine;

    public float waitTime;
    public int range;
    public int speed;
    public bool plantingState;
    public bool harvestingState;
    public bool isMoving;
    public bool tileFound;

    public List<OverlayTile> path = new List<OverlayTile>();
    public List<OverlayTile> tilledTiles = new List<OverlayTile>();
    public List<OverlayTile> inRangeTiles = new List<OverlayTile>();

    #region GAME MANAGER
    private void Awake()
    {
        GameManager.OnStateChange += GameManager_OnStateChange;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChange -= GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(GameManager.GameState state)
    {
        if (state == GameManager.GameState.PlantSeeds)
        {
            plantingState = true;
        }
    }
    #endregion

    private void Start()
    {
        villager = mouseController.villager;
        range = 0;
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        tilledTiles = mouseController.tilledTiles;
        //cropCountScript.cropValue = villager.crops;

        // start range detection
        GetInRangeTiles();
    }

    private void Update()
    {
        if (plantingState)
        {
            villager.plantingState = true;
            //seedCountScript.seedValue = villager.seeds;
            CheckPlant();
            CheckMove();
        }

    }

    private void CheckPlant()
    {
        // updates the GameState to HarvestSeeds if all tilled tiles have been planted a seed
        if (mouseController.villagerPlaced && villager.seeds == 0 && plantingState)
        {
            plantingState = false;
            Debug.Log("All seeds have been planted");
            GameManager.instance.UpdateGameState(GameManager.GameState.HarvestSeeds);
        }
    }

    private IEnumerator AddRange()
    {
        Debug.Log("Add range");
        // wait for waitTime seconds to perform the functions below
        yield return new WaitForSeconds(waitTime);

        // try and detect more tilled tiles by increasing the range in GetInRangeTiles()
        if (mouseController.tilledTiles.Count > 0 && !isMoving)
        {
            range++;
            GetInRangeTiles();
        }
    }

    private void RemoveRange()
    {
        // reset the current inRangeTiles list to a new list
        inRangeTiles = new List<OverlayTile>();
    }

    private void RangeDetection()
    {
        foreach (var tile in inRangeTiles)
        {
            // shows the Highlight gameObject under the overlayTile gameObject
            tile.HighlightTile();

            // generate the path using A* when a tile has been detected and
            // it is tilled and has no seed in it
            if (tile.isTilled && !tile.hasSeed)
            {
                tileFound = true;
                path = pathFinder.FindPath(villager.activeTile, tile);
                break;
            }
        }
    }

    private void HideHighlightRange()
    {
        foreach (var tile in inRangeTiles)
        {
            // more info in OverlayTile.cs/HideHighlightTile()
            tile.HideHighlightTile();
        }
    }

    private void GetInRangeTiles()
    {
        tileFound = false;
        // hides the Highlight gameObject under the overlayTile gameObject
        HideHighlightRange();

        // explained further in PathFinder.cs/GetTilesInRange()
        inRangeTiles = rangeFinder.GetTilesInRange(villager.activeTile, range);

        // looks for a tile in the list of inRangeTiles to plant on
        RangeDetection();

        // if tile is found:
        // stop increasing the range in GetTilesInRange(), and
        // stop looking for tilled tiles in range
        if (tileFound)
        {
            Debug.Log("Stop coroutine");
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        // if tile is not found still:
        // increase the range in GetTilesInRange(), and
        // continue looking for tilled tiles in range
        else if (!tileFound && !isMoving)
        {
            coroutine = StartCoroutine(AddRange());
        }
    }

    private bool CheckValidPath()
    {
        if (tileFound && path == null)
        {
            Debug.Log("No possible path found");
            HideHighlightRange();
            RemoveRange();
            // AudioManager.Instance.PlayHaggle();
            GameManager.instance.UpdateGameState(GameManager.GameState.GameOver);
            return false;
        }
        return true;
    }

    private void CheckMove()
    {
        if (!CheckValidPath()) return;
        // if the there is still a path from the pathfinding algo,
        // move towards the end tile
        if (path.Count > 0)
        {
            
            MoveAlongPath();
            isMoving = true;
        }

        if (tilledTiles.Count > 0 && tileFound && villager.activeTile.isTilled && !villager.activeTile.hasSeed)
        {
            // plant seed
            villager.activeTile.PlantSeed();
            villager.seeds--;
            isMoving = false;

            // reset range
            range = 1;

            // hide highlight range and remove past range
            HideHighlightRange();
            RemoveRange();

            // pop one tilled tile from the list
            tilledTiles.RemoveAt(0);

            if (tilledTiles.Count > 0) // get new path
            {
                Debug.Log("there are still " + tilledTiles.Count + " more tilled tiles");
                GetInRangeTiles();
            }
        }

        if (tilledTiles.Count > 0 && isMoving)
        {
            // if end tile is reached
            if (path.Count == 0)
            {
                // plant seed
                villager.activeTile.PlantSeed();
                villager.seeds--;
                isMoving = false;

                // reset range
                range = 1;

                // hide highlight range and remove past range
                HideHighlightRange();
                RemoveRange();

                // pop one tilled tile from the list
                tilledTiles.RemoveAt(0);
                if (tilledTiles.Count > 0) // get new path
                {
                    Debug.Log("there are still " + tilledTiles.Count + " more tilled tiles");
                    GetInRangeTiles();
                }
            }
        }
    }


    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;
        // move the villager along the path generated by the A* algorithm
        // using Unity's builtin MoveTowards() function
        // that takes in the villager's current position, the first element of the path list
        // aka where the villager is going, and a step, aka the speed with regards to time
        villager.transform.position = Vector2.MoveTowards(villager.transform.position, path[0].transform.position, step);

        // aligns the villager's position according to the path's position, aka its center
        if (Vector2.Distance(villager.transform.position, path[0].transform.position) < 0.001f)
        {
            // explained further in MouseController.cs/PositionCharacterOnTile()
            mouseController.PositionCharacterOnTile(path[0]);
            // remove the first element of the path list to move to the next tile
            path.RemoveAt(0);
        }
    }
}
