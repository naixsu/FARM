using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTile : MonoBehaviour
{
    #region PATHFINDING VARS
    public int _G;
    public int _H;
    public int _F;
    // public int _F { get { return _G + _H; } }
    public bool isBlocked;
    public OverlayTile previous;

    public Vector3Int gridLocation;
    public Vector2Int grid2DLocation { get { return new Vector2Int(gridLocation.x, gridLocation.y); } }
    #endregion


    [SerializeField] private Sprite overlayTile;
    /*[SerializeField] private Sprite plantedSprite;
    [SerializeField] private Sprite blocked;
    [SerializeField] private Sprite grass;
    [SerializeField] private Sprite till;
    [SerializeField] private Sprite growth1;
    [SerializeField] private Sprite growth2;
    [SerializeField] private Sprite growth3;
    [SerializeField] private Sprite growth4;
    [SerializeField] private Sprite harvested;*/

    public float plantGrowthTimer;
    public bool isTilled;
    public bool hasSeed;
    public bool isFullGrown;
    public bool isHarvested;
    private Coroutine coroutinePlantGrowth;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HideTile();
        }
    }

    /*public void CalculateFCost()
    {
        _F = _G + _H;
    }*/

    //
    // pretty self explanatory stuff here
    // 
    /*public void PlantSeed()
    {
        if (!hasSeed)
        {
            this.hasSeed = true;
            this.isHarvested = false;
            this.isFullGrown = false;
            this.GetComponent<SpriteRenderer>().sprite = plantedSprite;
            coroutinePlantGrowth = StartCoroutine(PlantGrowth());
            Debug.Log("Planted seed at tile " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            // AudioManager.Instance.PlayRandomPlant();
        }
    }*/

    /*public void HarvestCrop()
    {
        if (isFullGrown)
        {
            this.GetComponent<SpriteRenderer>().sprite = harvested;
            this.isHarvested = true;
            Debug.Log("Harvested crop at tile " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            // AudioManager.Instance.PlayHarvest();
        }
    }*/

    /*public IEnumerator PlantGrowth()
    {
        yield return new WaitForSeconds(plantGrowthTimer);
        this.GetComponent<SpriteRenderer>().sprite = growth1;
        yield return new WaitForSeconds(plantGrowthTimer);
        this.GetComponent<SpriteRenderer>().sprite = growth2;
        yield return new WaitForSeconds(plantGrowthTimer);
        this.GetComponent<SpriteRenderer>().sprite = growth3;
        yield return new WaitForSeconds(plantGrowthTimer);
        this.GetComponent<SpriteRenderer>().sprite = growth4;
        this.isFullGrown = true;
    }*/

    public void ShowTile(float alpha)
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
    }

    public void HighlightTile()
    {
        Transform childTransform = transform.GetChild(0);
        childTransform.gameObject.SetActive(true);
    }

    public void HideHighlightTile()
    {
        Transform childTransform = transform.GetChild(0);
        childTransform.gameObject.SetActive(false);
    }

    public void HideTile()
    {
        if (!isTilled)
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }

    /*public void TillTile(bool rand)
    {
        if (!isTilled)
        {
            this.isTilled = true;
            Debug.Log("Tilled tile at " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            this.GetComponent<SpriteRenderer>().sprite = till;
            this.ShowTile(1f);
            if (!rand) AudioManager.Instance.PlayRandomTillTile();
        }     
    }*/

    /*public void BlockTile(bool rand)
    {
        if (!isBlocked)
        {
            this.isBlocked = true;
            Debug.Log("Blocked tile at " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            this.GetComponent<SpriteRenderer>().sprite = blocked;
            this.ShowTile(1f);
            if (!rand) AudioManager.Instance.PlayRandomPlaceBlock();
        }
    }*/

    /*public void UnblockTile(bool rand)
    {
        if (isBlocked)
        {
            this.isBlocked = false;
            Debug.Log("Unblocked tile at " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            this.GetComponent<SpriteRenderer>().sprite = grass;
            if (!rand) AudioManager.Instance.PlayRandomBreakBlock();
        }
    }*/

    /*public void UntillTile(bool rand)
    {
        if (isTilled)
        {
            this.isTilled = false;
            Debug.Log("Untilled tile at " + this.gameObject.transform.position.x + " " + this.gameObject.transform.position.y);
            this.GetComponent<SpriteRenderer>().sprite = grass;
            this.HideTile();
            if (!rand) AudioManager.Instance.PlayRandomUnTillTile();
        }
    }*/
}
