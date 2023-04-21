using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    private bool mouseControl;

    public GameObject villagerPrefab;
    public VillagerInfo villager;
    public bool villagerButtonClicked;

    public Dictionary<Vector2Int, OverlayTile> map;
    public int seeds;
    public bool villagerPlaced = false;
    public bool isMoving;

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
        villagerButtonClicked = true;
    }

    private void Update()
    {
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
                            //AudioManager.Instance.PlayRandomVillagerIdleOnSpawn();
                            villagerPlaced = true;
                            //villagerButtonClicked = false;
                        }
                    }

                    if (villagerPlaced)
                    {
                        if (!overlayTile.isBlocked)
                        {
                            PositionCharacterOnTile(overlayTile);
                            //AudioManager.Instance.PlayRandomVillagerIdleOnSpawn();
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
                            //AudioManager.Instance.PlayVillagerDeath();
                            villagerPlaced = false;
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
        villager.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y - 0.05f, tile.transform.position.z+1);
        villager.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        villager.activeTile = tile;
    }
}
