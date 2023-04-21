using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HarvestManager : MonoBehaviour
{
    public MouseController mouseController;
    private VillagerInfo villager;
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private Coroutine coroutine;

    public int range;

    public bool plantingState;
    public bool harvestingState;
    public bool isMoving;
    public bool tileFound;

    public bool canPatrol;
    public bool patrolling;
    public int patrolRange;

     
    public List<OverlayTile> path = new List<OverlayTile>();
    public List<OverlayTile> grownTiles = new List<OverlayTile>();
    public List<OverlayTile> inRangeTiles = new List<OverlayTile>();

    public Dictionary<Vector2Int, OverlayTile> map;

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
        if (state == GameManager.GameState.HarvestSeeds)
        {
            harvestingState = true;
            
        }
    }
    #endregion

    private void Start()
    {
        
        villager = mouseController.villager;
        range = 0;
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        grownTiles = mouseController.toHarvest;
        map = MapManager.Instance.map;
        seedCountScript.seedValue = villager.seeds;


        // start range detection
        GetInRangeTiles();
    }

    private void Update()
    {
        if (harvestingState)
        {
            villager.plantingState = false;
            villager.harvestingState = true;
            cropCountScript.cropValue = villager.crops;
            CheckHarvest();
            CheckMove();
        }

        if (canPatrol)
        {
            StopCoroutine(coroutine);
            harvestingState = false;
            // OverlayTile randomTile = GetRandomTile();
            OverlayTile randomTile = GetRandomUnblockedTile();

            if (randomTile == villager.activeTile)
                return;
            path = pathFinder.FindPath(villager.activeTile, randomTile);
            
            canPatrol = false;
            if (path == null)
            {
                
                //Debug.Log("Null path");
                GetInRangeTiles();
                harvestingState = true;
                return;
            }
            patrolling = true;
        }

        if (patrolling)
            Patrol();

    }

    private void CheckHarvest()
    {
        // updates the GameState to HarvestSeeds if all tilled tiles have been planted a seed
        if (mouseController.villagerPlaced && grownTiles.Count == 0 && harvestingState)
        {
            harvestingState = false;
            //Debug.Log("All seeds have been harvested");
            GameManager.instance.UpdateGameState(GameManager.GameState.End);
        }
    }

    private OverlayTile GetRandomUnblockedTile()
    {
        List<OverlayTile> unblockedTiles = new List<OverlayTile>();
        foreach (OverlayTile tile in inRangeTiles)
        {
            if (!tile.isBlocked)
            {
                unblockedTiles.Add(tile);
            }
        }
        if (unblockedTiles.Count > 0)
        {
            OverlayTile randomTile = unblockedTiles[Random.Range(0, unblockedTiles.Count)];
            return randomTile;
        }
        else
        {
            return null; // no unblocked tile found
        }
    }

    private IEnumerator AddRange()
    {
        // wait for waitTime seconds to perform the functions below
        yield return new WaitForSeconds(villager.rangeWaitTime);

        // try and detect more tilled tiles by increasing the range in GetInRangeTiles()
        if (mouseController.toHarvest.Count > 0 && !isMoving)
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
            if (tile.isFullGrown && !tile.isHarvested)
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

    private void CanPatrol()
    {
        if (!canPatrol && !isMoving && !tileFound)
        {
            int rng = Random.Range(0, patrolRange);
            //Debug.Log("Try Patrol " + rng);

            // canPatrol is 1 in patrolRange chance
            canPatrol = (rng == 1 && inRangeTiles.Count > 1);
            if (rng == 1) AudioManager.Instance.PlayRandomVillagerIdle();
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
            //Debug.Log("Stop coroutine");
            if (coroutine != null)
                StopCoroutine(coroutine);
            
        }
        // if tile is not found still:
        // increase the range in GetTilesInRange(), and
        // continue looking for tilled tiles in range
        else if (!tileFound && !isMoving)
        {
            coroutine = StartCoroutine(AddRange());
            // Patrol
            CanPatrol();
        }
    }

    private void Patrol()
    {
        
        // if the there is still a path from the pathfinding algo,
        // move towards the end tile
        if (path != null && path.Count > 0)
        {
            MoveAlongPath();
            isMoving = true;
        }

        if (grownTiles.Count > 0 && isMoving && patrolling)
        {
            // if end tile is reached
            if (path.Count == 0)
            {
                canPatrol = false;
                patrolling = false;
                isMoving = false;

                // reset range
                range = 1;

                // hide highlight range and remove past range
                HideHighlightRange();
                RemoveRange();

                if (grownTiles.Count > 0) // get new path
                {
                    harvestingState = true;
                    //Debug.Log("there are still " + grownTiles.Count + " more crops");
                    GetInRangeTiles();
                }
            }
        }
    }

    private void CheckMove()
    {
        // if the there is still a path from the pathfinding algo,
        // move towards the end tile
        if (grownTiles.Count > 0 && tileFound && villager.activeTile.isFullGrown && !villager.activeTile.isHarvested)
        {
            //Debug.Log("Here");
            canPatrol = false;
            // harvest crop
            villager.activeTile.HarvestCrop();
            villager.crops++;
            isMoving = false;

            // reset range
            range = 1;

            // hide highlight range and remove past range
            HideHighlightRange();
            RemoveRange();

            // pop one tilled tile from the list

            grownTiles.RemoveAt(0);
            if (grownTiles.Count > 0) // get new path
            {
                //Debug.Log("there are still " + grownTiles.Count + " more crops");
                GetInRangeTiles();
            }
        }

        if (path != null && path.Count > 0)
        {
            MoveAlongPath();
            isMoving = true;
        }

        

        if (grownTiles.Count > 0 && isMoving)
        {
            // if end tile is reached
            if (path.Count == 0)
            {
                canPatrol = false;
                // harvest crop
                villager.activeTile.HarvestCrop();
                
                villager.crops++;
                isMoving = false;

                // reset range
                range = 1;

                // hide highlight range and remove past range
                HideHighlightRange();
                RemoveRange();

                // pop one tilled tile from the list

                grownTiles.RemoveAt(0);
                if (grownTiles.Count > 0) // get new path
                {
                    //Debug.Log("there are still " + grownTiles.Count + " more crops");
                    GetInRangeTiles();
                }
            }
        }
    }


    private void MoveAlongPath()
    {
        var step = villager.speed * Time.deltaTime;
        // move the villager along the path generated by the A* algorithm
        // using Unity's builtin MoveTowards() function
        // that takes in the villager's current position, the first element of the path list
        // aka where the villager is going, and a step, aka the speed with regards to time
        villager.transform.position = Vector3.MoveTowards(villager.transform.position, path[0].transform.position, step);

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
