using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    private static MapManager instance;
    public static MapManager Instance { get { return instance; } }

    public OverlayTile overlayTilePrefab;
    public GameObject overlayContainer;

    [SerializeField] private Tilemap tileMap;

    public Dictionary<Vector2Int, OverlayTile> map;

    [SerializeField] private Camera _camera;
    [SerializeField] private float _padding = 1f;
    public float zoomSpeed;
    public float minZoom;
    public float maxZoom;
    public float cameraMoveSpeed;

    public bool ignoreBottomTiles;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        GameManager.OnStateChange += GameManager_OnStateChange;

    }

    private void OnDestroy()
    {
        GameManager.OnStateChange -= GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(GameManager.GameState state)
    {
        if (state == GameManager.GameState.SetUp)
        {
            SetUp();
        }
    }

    // Start is called before the first frame update
    private void Update()
    {
        // Zoom in and out using scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _camera.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);

        /*// Move the camera using mouse3
        if (Input.GetMouseButton(2))
        {
            Vector3 move = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f) * cameraMoveSpeed;
            transform.position += move;
        }*/
    }

    void SetUp()
    {
        Debug.Log("Setting Up");
        // get a dictionary of all the tiles in the screen
        map = new Dictionary<Vector2Int, OverlayTile>();

        BoundsInt bounds = tileMap.cellBounds;

        var count = 0;

        for (int z = bounds.max.z; z >= bounds.min.z; z--)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {
                    if (z == 0 && ignoreBottomTiles)
                        return;

                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector2Int(x, y);

                    // add to dictionary
                    if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                    {
                        // instantiate the gameObject
                        var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                        // change the name for better tracking purposes
                        overlayTile.name = overlayTile.name + "_" + count++;
                        // get the coordinates for the tile in the scene
                        var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                        // assign the gameObject's position according to its tileMap position
                        overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                        // adjust the sprite's sorting order
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                        // assign its gridLocation to be used in pathfinding
                        overlayTile.gridLocation = tileLocation;
                        // add to dictionary
                        map.Add(tileKey, overlayTile);
                    }
                }
            }
        }

        ResizeCameraToMap(3f);
        // switch GameState once setup is finished
        Debug.Log("Finished Setting Up");
        GameManager.instance.UpdateGameState(GameManager.GameState.MouseControl);

    }

    private void ResizeCameraToMap(float zoomOut = 0f)
    {
        Debug.Log("Resizing");
        Vector3 bottomLeftCorner = new Vector3(float.MaxValue, float.MaxValue, 0f);
        Vector3 topRightCorner = new Vector3(float.MinValue, float.MinValue, 0f);

        // Find the bounds of the map
        foreach (KeyValuePair<Vector2Int, OverlayTile> tile in map)
        {
            Vector3 tilePosition = tile.Value.transform.position;

            // Adjust the tile position to account for the isometric grid
            tilePosition.x = (tilePosition.x - tilePosition.y) / 2f;
            tilePosition.y = (tilePosition.x + tilePosition.y) / 2f;

            if (tilePosition.x < bottomLeftCorner.x)
            {
                bottomLeftCorner.x = tilePosition.x;
            }

            if (tilePosition.y < bottomLeftCorner.y)
            {
                bottomLeftCorner.y = tilePosition.y;
            }

            if (tilePosition.x > topRightCorner.x)
            {
                topRightCorner.x = tilePosition.x;
            }

            if (tilePosition.y > topRightCorner.y)
            {
                topRightCorner.y = tilePosition.y;
            }
        }

        // Calculate the camera's new position and size
        Vector3 cameraPosition = (bottomLeftCorner + topRightCorner) / 2f;
        float cameraSize = Mathf.Max((topRightCorner.y - bottomLeftCorner.y) / 2f, (topRightCorner.x - bottomLeftCorner.x) / 2f / _camera.aspect) + _padding;

        // Zoom out the camera
        cameraSize += zoomOut;

        // Set the camera's position and size
        _camera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, _camera.transform.position.z);
        _camera.orthographicSize = cameraSize;
    }

    public List<OverlayTile> PathNeighbors(OverlayTile currentOverlayTile)
    {

        // list of neighboring tiles top, down, left, right, topleft, topright, downleft, downright
        List<OverlayTile> neighbors = new List<OverlayTile>();
        OverlayTile topTile = null;
        OverlayTile bottomTile = null;
        OverlayTile leftTile = null;
        OverlayTile rightTile = null;

        #region GET NEIGHBORS

        // top
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            topTile = neighborTile;
            //if (!neighborTile.isBlocked)
            neighbors.Add(neighborTile);
        }

        // left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            leftTile = neighborTile;
            //if (!neighborTile.isBlocked)
            neighbors.Add(neighborTile);
        }

        // down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            bottomTile = neighborTile;
            //if (!neighborTile.isBlocked)
            neighbors.Add(neighborTile);
        }

        // right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            rightTile = neighborTile;
            //if (!neighborTile.isBlocked)
            neighbors.Add(neighborTile);
        }


        // top left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((topTile == null || !topTile.isBlocked) && (leftTile == null || !leftTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            /*if ((!topTile?.isBlocked ?? true) || (!leftTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);*/
        }

        // left down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((bottomTile == null || !bottomTile.isBlocked) && (leftTile == null || !leftTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            /*if ((!bottomTile?.isBlocked ?? true) || (!leftTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);*/
        }

        // down right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((bottomTile == null || !bottomTile.isBlocked) && (rightTile == null || !rightTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            /*if ((!bottomTile?.isBlocked ?? true) || (!rightTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);*/
        }

        // right iop
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((topTile == null || !topTile.isBlocked) && (rightTile == null || !rightTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            /*if ((!topTile?.isBlocked ?? true) || (!rightTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);*/
        }

        #endregion

        return neighbors;
    }


    public List<OverlayTile> NewOrderGetNeighborTiles(OverlayTile currentOverlayTile)
    {

        // list of neighboring tiles top, down, left, right, topleft, topright, downleft, downright
        List<OverlayTile> neighbors = new List<OverlayTile>();
        OverlayTile topTile = null;
        OverlayTile bottomTile = null;
        OverlayTile leftTile = null;
        OverlayTile rightTile = null;

        #region GET NEIGHBORS

        // top
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if (!neighborTile.isBlocked)
            {
                topTile = neighborTile;
                neighbors.Add(neighborTile);
            }

        }

        // left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if (!neighborTile.isBlocked)
            {
                leftTile = neighborTile;
                neighbors.Add(neighborTile);
            }

        }

        // down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if (!neighborTile.isBlocked)
            {
                bottomTile = neighborTile;
                neighbors.Add(neighborTile);
            }

        }

        // right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];

            if (!neighborTile.isBlocked)
            {
                rightTile = neighborTile;
                neighbors.Add(neighborTile);
            }

        }

        // top left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck) && !map[locationToCheck].isBlocked)
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((topTile == null || !topTile.isBlocked) && (leftTile == null || !leftTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            if ((!topTile?.isBlocked ?? true) || (!leftTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);
        }

        // left down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck) && !map[locationToCheck].isBlocked)
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((bottomTile == null || !bottomTile.isBlocked) && (leftTile == null || !leftTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            if ((!bottomTile?.isBlocked ?? true) || (!leftTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);
        }

        // down right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck) && !map[locationToCheck].isBlocked)
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((bottomTile == null || !bottomTile.isBlocked) && (rightTile == null || !rightTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            if ((!bottomTile?.isBlocked ?? true) || (!rightTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);
        }

        // right iop
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck) && !map[locationToCheck].isBlocked)
        {
            OverlayTile neighborTile = map[locationToCheck];

            if ((topTile == null || !topTile.isBlocked) && (rightTile == null || !rightTile.isBlocked))
            {
                neighbors.Add(neighborTile);
            }

            if ((!topTile?.isBlocked ?? true) || (!rightTile?.isBlocked ?? true))
                neighbors.Add(neighborTile);
        }

        #endregion

        return neighbors;
    }


}