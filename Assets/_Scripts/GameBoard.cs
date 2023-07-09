using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class GameBoard : StaticInstance<GameBoard> 
{
    public enum TileType {
        Empty, Unclaimed, Available, Occupied 
    }

    #region Tilemap References
        [Header("Tilemaps")]
        private GridLayout _grid;
        [SerializeField] Tilemap _temp;
        [SerializeField] Tilemap _logic;
        [SerializeField] Tilemap _graphics;
    #endregion

    private static Dictionary<TileType, TileBase> tileOccupation = new Dictionary<TileType, TileBase>();
    private static Dictionary<Terrain, TerrainTile> tileBases = new Dictionary<Terrain, TerrainTile>();

    private Vector3Int[] adjacencies;
    private Vector3Int[] range2Adjacencies;

    public int HumanMovesPerTurn {get; private set;}

    public BoundsInt CellBounds {
        get=>_logic.cellBounds;
    }
    public int UnclaimedTiles {
        get => _logic.GetTilesBlock(CellBounds).Count(t => t = tileOccupation[TileType.Unclaimed]);
    }

    #region Monobehaviour Methods

        protected override void Awake() 
        {
            base.Awake();  
            
            string tilePath = @"Tiles\";
            string availabiltyPath = tilePath + @"Availability\";
            string terrainPath = tilePath + @"Terrain\";

            tileOccupation.Add(TileType.Empty, null);
            tileOccupation.Add(TileType.Unclaimed, Resources.Load<TileBase>(availabiltyPath+"White Iso-Tile"));
            tileOccupation.Add(TileType.Available, Resources.Load<TileBase>(availabiltyPath+"Green Iso-Tile"));
            tileOccupation.Add(TileType.Occupied, Resources.Load<TileBase>(availabiltyPath+"Red Iso-Tile"));

            tileBases.Add(Terrain.Plains, Resources.Load<TerrainTile>(terrainPath+"Plains"));
            tileBases.Add(Terrain.Forest, Resources.Load<TerrainTile>(terrainPath+"Forest"));
            tileBases.Add(Terrain.Mountain, Resources.Load<TerrainTile>(terrainPath+"Mountain"));
            tileBases.Add(Terrain.Water, Resources.Load<TerrainTile>(terrainPath+"Water"));
            tileBases.Add(Terrain.City, Resources.Load<TerrainTile>(terrainPath+"City"));
            tileBases.Add(Terrain.Mine, Resources.Load<TerrainTile>(terrainPath+"Mine"));
            tileBases.Add(Terrain.Farm, Resources.Load<TerrainTile>(terrainPath+"Farm"));

        }
        private void Start() 
        {
            adjacencies = new Vector3Int[] {
                Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left 
            };

            range2Adjacencies = new Vector3Int[] {
                Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left,
                Vector3Int.up + Vector3Int.right, Vector3Int.down + Vector3Int.left,
                Vector3Int.up - Vector3Int.right, Vector3Int.down - Vector3Int.left,
                2*Vector3Int.up, 2*Vector3Int.right, 2*Vector3Int.down, 2*Vector3Int.left
            };

            HumanMovesPerTurn = 2;
        }
    #endregion

    public void EnableHelperGrid(bool enabled=true) 
    {
        _temp.gameObject.SetActive(enabled);
        _logic.gameObject.SetActive(enabled);
    }

    public void ClearTemp()
    {
        Vector3Int cellPosition;
        BoundsInt cellBounds = _temp.cellBounds;

        for (int y = cellBounds.yMin; y < cellBounds.yMax; y++)
        {
            for (int x = cellBounds.xMin; x < cellBounds.xMax; x++)
            {
                cellPosition = new Vector3Int(x,y,0);
                _temp.SetTile(cellPosition, tileOccupation[TileType.Empty]);
            }
        }
    }

    public bool Validate(Vector3 position, TerrainTile tile, bool bypassOccupancy=false) 
    {
        if (tile == null) return false;

        Vector3Int cellPosition = _logic.WorldToCell(position);

        bool tileIsUnclaimed = _logic.GetTile(cellPosition) == tileOccupation[TileType.Unclaimed];
        bool claimBypassed = bypassOccupancy && _logic.GetTile(cellPosition) != null;

        switch(tile.terrain)
        {
            // Nature Tile Validations
            case Terrain.Forest:
            case Terrain.Mountain:
            case Terrain.Water:
                if (tileIsUnclaimed || claimBypassed)
                {
                    _temp.SetTile(cellPosition, tileOccupation[TileType.Available]);
                    return true;
                }
                else
                {
                    _temp.SetTile(cellPosition, tileOccupation[TileType.Occupied]);
                    return false;
                }

                // bool tileIsHuman = new Terrain[] {Terrain.City, Terrain.Mine, Terrain.Farm}.Contains(
                //         _graphics.GetTile<TerrainTile>(cellPosition).terrain
                // );
                // if (tileIsUnclaimed || tileIsHuman || claimBypassed)
                // {
                //     _temp.SetTile(cellPosition, tileOccupation[TileType.Available]);
                //     return true;
                // }
                // else
                // {
                //     _temp.SetTile(cellPosition, tileOccupation[TileType.Occupied]);
                //     return false;
                // }
            
            // Human Tile Validations
            case Terrain.City:
                bool isAdjacentToHumans = adjacencies.Any(dir => 
                    new Terrain[] {Terrain.City, Terrain.Mine, Terrain.Farm}.Contains(
                        _graphics.GetTile<TerrainTile>(cellPosition+dir).terrain
                    )
                );
                return (tileIsUnclaimed || claimBypassed) && isAdjacentToHumans;

            case Terrain.Mine:
                bool isAdjacentToMountains = range2Adjacencies.Any(dir => 
                    _graphics.GetTile<TerrainTile>(cellPosition+dir).terrain == Terrain.Mountain
                );
                return (tileIsUnclaimed || claimBypassed) && isAdjacentToMountains;

            case Terrain.Farm:
                bool isAdjacentToWater = range2Adjacencies.Any(dir => 
                    _graphics.GetTile<TerrainTile>(cellPosition+dir).terrain == Terrain.Water
                );
                return (tileIsUnclaimed || claimBypassed) && isAdjacentToWater;

            default:
            return true;
        }

        // if (_logic.GetTile(cellPosition) == tileOccupation[TileType.Unclaimed])
        // {
        //     _temp.SetTile(cellPosition, tileOccupation[TileType.Available]);
        //     return true;
        // }
        // else 
        // {
        //     _temp.SetTile(cellPosition, tileOccupation[TileType.Occupied]);
        //     return false;
        // }
    }

    public void Submit(Vector3 position, TerrainTile tile, bool bypassOccupancy=false)
    {
        if (!Validate(position, tile, bypassOccupancy)) return;
        
        Vector3Int cellPosition = _logic.WorldToCell(position);
        Vector3Int[] adjacencies = new Vector3Int[] {
            Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left 
        };

        if (tile.terrain != Terrain.Plains)
            _logic.SetTile(cellPosition, tileOccupation[TileType.Occupied]);
        else
            _logic.SetTile(cellPosition, tileOccupation[TileType.Unclaimed]);

        _graphics.SetTile(cellPosition, tile);


        // Check for side effects from the tile placed
        TerrainTile currentTile;
        Terrain[] humanTiles;
        switch (tile.terrain)
        {
            case Terrain.Mountain:
                foreach (Vector3Int dir in adjacencies)
                {
                    currentTile = _graphics.GetTile<TerrainTile>(cellPosition+dir);
                    humanTiles = new Terrain[] {Terrain.City, Terrain.Mine, Terrain.Farm};
                    
                    if (currentTile != null && humanTiles.Contains(currentTile.terrain))
                    {
                        Submit(_logic.CellToWorld(cellPosition+dir), tileBases[Terrain.Plains]);
                    }    
                }
                break;

            case Terrain.City:
                foreach (Vector3Int dir in adjacencies)
                {
                    currentTile = _graphics.GetTile<TerrainTile>(cellPosition+dir);

                    if (currentTile != null && currentTile.terrain == Terrain.Forest)
                    {
                        int adjacentCities = adjacencies.Count(dir_f => 
                            _graphics.GetTile<TerrainTile>(cellPosition+dir+dir_f).terrain == Terrain.City
                        );

                        if (adjacentCities >= 2)
                            Submit(_logic.CellToWorld(cellPosition+dir), tileBases[Terrain.City], true);
                    }
                }
                break;

            case Terrain.Mine:
            case Terrain.Farm:
                HumanMovesPerTurn++;
                break;

            case Terrain.Plains:
                currentTile = _graphics.GetTile<TerrainTile>(cellPosition);
                humanTiles = new Terrain[] {Terrain.Mine, Terrain.Farm};
                
                if (humanTiles.Contains(currentTile.terrain)) 
                    HumanMovesPerTurn = Mathf.Max(2, HumanMovesPerTurn-1);
                break;

            default: break;
        }
    }
}