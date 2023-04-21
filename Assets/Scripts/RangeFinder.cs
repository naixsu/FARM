using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangeFinder
{
    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range)
    {
        // initialize
        var inRangeTiles = new List<OverlayTile>();
        // the range of tiles to get tiles in range
        int stepCount = 0;
        inRangeTiles.Add(startingTile);
        var tileForPreviousStep = new List<OverlayTile>();
        tileForPreviousStep.Add(startingTile);


        // basically just get the neighboring tiles
        // but range increases
        // if range increases, the neighboring tiles of each neighbor will also be returned
        // and so on
        while (stepCount < range)
        {
            var neighborTiles = new List<OverlayTile>();

            foreach (var neighborTile in inRangeTiles)
            {
                neighborTiles.AddRange(MapManager.Instance.NewOrderGetNeighborTiles(neighborTile));
            }

            inRangeTiles.AddRange(neighborTiles);
            // tileForPreviousStep = neighborTiles.Distinct().ToList();
            inRangeTiles = inRangeTiles.Distinct().ToList();
            PrintInrange(inRangeTiles);
            stepCount++;
        }

        // return a list of distinct tiles
        return inRangeTiles.Distinct().ToList();
    }

    void PrintInrange(List<OverlayTile> inRangeTiles)
    {
        Debug.Log("Printing Tiles");
        foreach (var tile in inRangeTiles)
        {
            Debug.Log(tile.grid2DLocation);
        }
    }
}
