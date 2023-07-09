using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Terrain 
{
    Plains, Forest, Mountain, Water, City, Mine, Farm
}

[CreateAssetMenu(fileName="New Terrain Tile", menuName = "Tiles/TerrainTiles")]
public class TerrainTile : Tile
{
    public Terrain terrain = Terrain.Plains;
}
