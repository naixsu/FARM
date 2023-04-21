using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MouseController : MonoBehaviour
{
    public bool mouseControl;

    public GameObject villagerPrefab;
    public VillagerInfo villager;
    
    public Dictionary<Vector2Int, OverlayTile> map;
    public int seeds;
    public bool villagerPlaced = false;
    public bool isMoving;

    public GameObject spawnSeedsWarning;
    [SerializeField] private Button _spawnSeedsButton;

    public bool villagerButtonClicked;
    [SerializeField] private Button _villagerButton;
    [SerializeField] private Sprite defaultVillager;
    [SerializeField] private Sprite villagerSelected;

    public bool hoeButtonClicked = false;
    [SerializeField] private Button _hoeButton;
    [SerializeField] private Sprite defaultHoe;
    [SerializeField] private Sprite hoeSelected;

    public bool pickaxeButtonClicked = false;
    [SerializeField] private Button _pickaxeButton;
    [SerializeField] private Sprite defaultPickaxe;
    [SerializeField] private Sprite pickaxeSelected;

    public bool randomizeButtonClicked;
    [SerializeField] private Button _randomizeButton;
    [SerializeField] private Sprite randomizeButtonSelected;
    [SerializeField] private Sprite defaultRandomizeButton;

    public List<OverlayTile> tilledTiles = new List<OverlayTile>();
    public List<OverlayTile> toHarvest = new List<OverlayTile>();

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
        if (state == GameManager.GameState.MouseControl)
        {
            mouseControl = true;
        }
    }

    #endregion

    private void Start()
    {
        map = MapManager.Instance.map;
        seeds = 0;
    }
    private void Update()
    {
        ButtonClick();
        if (villagerPlaced)
        {
            villager.seeds = seeds;
        }
        seedCountScript.seedValue = seeds;

        if (mouseControl)
        {
            var focusedTileHit = GetFocusedOnTile();
            if (focusedTileHit.HasValue)
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
                // get the gameObject the raycast has hit
                OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();
                // set this gameObject's position according to the overlayTile's position
                // this also changes the Cursor sprite's position, which makes it look like
                // the cursor is selecting other tiles
                transform.position = overlayTile.transform.position;
                // adjust this sorting order
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 1;

                #region VILLAGER BUTTON PRESS
                if (Input.GetMouseButtonDown(1) && villagerButtonClicked)
                {
                    if (!villagerPlaced)
                    {
                        if (villager == null && !overlayTile.isBlocked)
                        {
                            // if the villager is not placed and there is no villager prefab in the scene,
                            // instantiate the villager
                            villager = Instantiate(villagerPrefab).GetComponent<VillagerInfo>();
                            // basically positions the villager's transform.position according to the overlayTile's transform.position
                            // set the villager's active tile to the overlayTile detected
                            PositionCharacterOnTile(overlayTile);
                            AudioManager.Instance.PlayRandomVillagerIdleOnSpawn();
                            villagerPlaced = true;
                            //villagerButtonClicked = false;
                        }
                    }

                    if (villagerPlaced)
                    {
                        if (!overlayTile.isBlocked)
                        {
                            PositionCharacterOnTile(overlayTile);
                            AudioManager.Instance.PlayRandomVillagerIdleOnSpawn();
                            villagerPlaced = true;
                        }
                    }
                }

                else if (Input.GetMouseButton(0) && villagerButtonClicked)
                {
                    if (villagerPlaced)
                    {
                        if (overlayTile == villager.activeTile)
                        {
                            Debug.Log("Destroy");
                            Destroy(villager.gameObject);
                            AudioManager.Instance.PlayVillagerDeath();
                            villagerPlaced = false;
                        }
                    }
                }
                #endregion

                #region HOE BUTTON PRESS
                if (Input.GetMouseButton(1) && hoeButtonClicked)
                {
                    if (!overlayTile.isTilled && !overlayTile.isBlocked)
                    {
                        // click to till tiles
                        overlayTile.TillTile(false);

                        // add the villager's seed count accordingly
                        if (tilledTiles.Count >= 0)
                        {
                            //villager.seeds++;
                            seeds++;
                            Debug.Log("seeds " + seeds);
                        }
                        // add tilled tiles to the list
                        tilledTiles.Add(overlayTile);
                        toHarvest.Add(overlayTile);
                    }
                }

                else if (Input.GetMouseButton(0) && hoeButtonClicked)
                {
                    if (overlayTile.isTilled && !overlayTile.isBlocked)
                    {
                        // can click to untill a tile (if tilled)
                        overlayTile.UntillTile(false);

                        // deduct the villager's seed count accordingly
                        if (tilledTiles.Count > 0)
                            //villager.seeds--;
                            seeds--;
                        // remove tile to the list
                        tilledTiles.Remove(overlayTile);
                        toHarvest.Remove(overlayTile);
                    }
                }
                #endregion

                #region PICKAXE BUTTON PRESS
                if (Input.GetMouseButton(1) && pickaxeButtonClicked)
                {
                    if (villagerPlaced)
                    {
                        if ((overlayTile.isTilled && villager.activeTile != overlayTile) ||
                            (!overlayTile.isTilled && villager.activeTile != overlayTile))
                        {
                            Debug.Log("Override");
                            // if a tilled tile is selected, replace with obstacle
                            overlayTile.BlockTile(false);

                            // deduct the villager's seed count accordingly
                            if (tilledTiles.Count > 0 && overlayTile.isTilled) seeds--;
                            // remove tile to the list
                            tilledTiles.Remove(overlayTile);
                            toHarvest.Remove(overlayTile);
                            // overlayTile.isBlocked = true;
                        }
                    }

                    if (!villagerPlaced)
                    {
                        if (overlayTile.isTilled || !overlayTile.isBlocked)
                        {
                            Debug.Log("Override");
                            // if a tilled tile is selected, replace with obstacle
                            overlayTile.BlockTile(false);
                            AudioManager.Instance.PlayRandomPlaceBlock();
                            // deduct the villager's seed count accordingly
                            if (tilledTiles.Count > 0 && overlayTile.isTilled) seeds--;
                            // remove tile to the list
                            tilledTiles.Remove(overlayTile);
                            toHarvest.Remove(overlayTile);
                        }
                    }
                }

                else if (Input.GetMouseButton(0) && pickaxeButtonClicked)
                {
                    if (villagerPlaced)
                    {
                        if (overlayTile.isBlocked && villager.activeTile != overlayTile)
                        {
                            overlayTile.UnblockTile(false);
                        }
                    }
                    if (!villagerPlaced)
                    {
                        if (overlayTile.isBlocked)
                        {
                            overlayTile.UnblockTile(false);
                        }
                    }
                }



                #endregion

            }
            else
            {
                // disable cursor sprite once cursor is outside tiles
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    private void ButtonClick()
    {
        if (hoeButtonClicked)
        {
            _hoeButton.GetComponent<Image>().sprite = hoeSelected;
            _villagerButton.GetComponent<Image>().sprite = defaultVillager;
            _pickaxeButton.GetComponent<Image>().sprite = defaultPickaxe;
        }

        if (pickaxeButtonClicked)
        {
            _hoeButton.GetComponent<Image>().sprite = defaultHoe;
            _villagerButton.GetComponent<Image>().sprite = defaultVillager;
            _pickaxeButton.GetComponent<Image>().sprite = pickaxeSelected;
        }

        if (villagerButtonClicked)
        {
            _hoeButton.GetComponent<Image>().sprite = defaultHoe;
            _villagerButton.GetComponent<Image>().sprite = villagerSelected;
            _pickaxeButton.GetComponent<Image>().sprite = defaultPickaxe;
        }
    }


    public void villagerButton()
    {
        villagerButtonClicked = !villagerButtonClicked;
        hoeButtonClicked = false;
        pickaxeButtonClicked = false;
    }
    public void pickaxeButton()
    {
        pickaxeButtonClicked = !pickaxeButtonClicked;
        villagerButtonClicked = false;
        hoeButtonClicked = false;
    }
    public void hoeButton()
    {
        hoeButtonClicked = !hoeButtonClicked;
        villagerButtonClicked = false;
        pickaxeButtonClicked = false;
    }
    public void spawnSeedsButton()
    {

        // updates GameState to PlantSeeds if there are one or more tilled tiles in the screen
        if (tilledTiles.Count > 0 && villagerPlaced)
        {
            mouseControl = false;
            GameManager.instance.UpdateGameState(GameManager.GameState.PlantSeeds);
        }

        else
        {
            Debug.Log("No tilled tiles to plant on");
            GameObject SSWGO = Instantiate(spawnSeedsWarning, _spawnSeedsButton.transform);
            StartCoroutine(DestroySSWGO(SSWGO));
        }

        villagerButtonClicked = false;
        pickaxeButtonClicked = false;
        hoeButtonClicked = false;
    }

    IEnumerator DestroySSWGO(GameObject SSWGO)
    {
        Debug.Log("Destroying SSWGO");
        yield return new WaitForSeconds(0.5f);
        Destroy(SSWGO);
    }

    public void Randomize()
    {
        villagerButtonClicked = false;
        pickaxeButtonClicked = false;
        hoeButtonClicked = false;
        randomizeButtonClicked = true;

        Debug.Log("Randomizing");
        _randomizeButton.GetComponent<Image>().sprite = randomizeButtonSelected;
        _hoeButton.GetComponent<Image>().sprite = defaultHoe;
        _villagerButton.GetComponent<Image>().sprite = defaultVillager;
        _pickaxeButton.GetComponent<Image>().sprite = defaultPickaxe;

        RandomVillager();
        RandomTilledTiles();
        RandomBlockedTiles();

        StartCoroutine(ResetRandomizeButton());
    }

    private IEnumerator ResetRandomizeButton()
    {
        yield return new WaitForSeconds(0.2f);
        randomizeButtonClicked = false;
        _randomizeButton.GetComponent<Image>().sprite = defaultRandomizeButton;
    }
    private void RandomVillager()
    {
        // Randomize Villager position
        // Destroy previous villager if exists
        if (villagerPlaced) Destroy(villager.gameObject);

        Vector2Int randomVillagerPos = GetRandomMapPosition();
        OverlayTile villagerTile = map[randomVillagerPos];

        villager = Instantiate(villagerPrefab).GetComponent<VillagerInfo>();

        PositionCharacterOnTile(villagerTile);
        AudioManager.Instance.PlayRandomVillagerIdleOnSpawn();
        villagerPlaced = true;
    }

    private void RandomTilledTiles()
    {
        // Destroy tilled tiles if exists
        seeds = 0;
        foreach (KeyValuePair<Vector2Int, OverlayTile> tile in map)
        {
            OverlayTile tileInfo = tile.Value;

            if (tileInfo.isTilled)
            {
                tileInfo.UntillTile(true);
                tilledTiles.Remove(tileInfo);
                toHarvest.Remove(tileInfo);
            }

        }
        // Randomize Tilled Tiles
        int numTilledTiles = Random.Range(1, map.Count);
        for (int i = 0; i < numTilledTiles; i++)
        {
            Vector2Int randomTilledPos = GetRandomMapPosition();
            OverlayTile tilledTile = map[randomTilledPos];
            if (!tilledTile.isBlocked && !tilledTile.isTilled)
            {
                tilledTile.TillTile(true);
                seeds++;
                tilledTiles.Add(tilledTile);
                toHarvest.Add(tilledTile);
            }
        }

        AudioManager.Instance.PlayRandomTillTile();

    }

    private void RandomBlockedTiles()
    {
        // Destroy blocked tiles if exists
        foreach (KeyValuePair<Vector2Int, OverlayTile> tile in map)
        {
            OverlayTile tileInfo = tile.Value;
            if (tileInfo.isBlocked)
            {
                tileInfo.UnblockTile(true);
            }

        }
        // Randomize Tilled Tiles
        int numBlockedTiles = Random.Range(1, tilledTiles.Count);
        for (int i = 0; i < numBlockedTiles; i++)
        {
            Vector2Int randomTilledPos = GetRandomMapPosition();
            OverlayTile blockedTile = map[randomTilledPos];
            if (!blockedTile.isBlocked && blockedTile != villager.activeTile && !blockedTile.isTilled)
            {
                blockedTile.BlockTile(true);
                tilledTiles.Remove(blockedTile);
            }
        }

        AudioManager.Instance.PlayRandomPlaceBlock();
    }

    private Vector2Int GetRandomMapPosition()
    {
        // Get a random tile from the dictionary
        KeyValuePair<Vector2Int, OverlayTile> randomTile = map.ElementAt(Random.Range(0, map.Count));
        return randomTile.Key;

    }


    public RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
        if (hits.Length > 0)
        {
            // returns the topmost component/gameObject/whatever the raycast hits
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    public void PositionCharacterOnTile(OverlayTile tile)
    {
        villager.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z+1);
        // villager.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder + 5;
        villager.activeTile = tile;
    }
}
