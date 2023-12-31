using System;
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
    public static Dictionary<Terrain, TerrainTile> tileBases = new Dictionary<Terrain, TerrainTile>();

    private Vector3Int[] _adjacencies;
    private Vector3Int[] _range2Adjacencies;
    private readonly Terrain[] humanTerrains = new Terrain[] {Terrain.City, Terrain.Mine, Terrain.Farm};

    public int NatureScore {get; private set;}
    public int HumanScore {get; private set;}
    public int HumanMovesPerTurn {get; private set;}

    public BoundsInt CellBounds {
        get=>_logic.cellBounds;
    }
    public int UnclaimedTiles {
        get => _logic.GetTilesBlock(CellBounds).Count(t => t == tileOccupation[TileType.Unclaimed]);
    }

    public Action<Vector3Int,TerrainTile> OnResourceAdded;
    public Action<int> OnHumansScoreChanged;
    public Action<int> OnNatureScoreChanged;

    #region Monobehaviour Methods

        protected override void Awake() 
        {
            base.Awake();  
            
            string tilePath = @"Tiles\";
            string availabiltyPath = tilePath + @"Availability\";
            string terrainPath = tilePath + @"Terrain\";

            tileOccupation.Clear();
            tileBases.Clear();

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
            _adjacencies = new Vector3Int[] {
                Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left 
            };

            _range2Adjacencies = new Vector3Int[] {
                Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left,
                Vector3Int.up + Vector3Int.right, Vector3Int.down + Vector3Int.left,
                Vector3Int.up - Vector3Int.right, Vector3Int.down - Vector3Int.left,
                2*Vector3Int.up, 2*Vector3Int.right, 2*Vector3Int.down, 2*Vector3Int.left
            };

            HumanMovesPerTurn = 2;
            NatureScore = 0;
            HumanScore = 0;
        }
    #endregion

    public List<Vector3Int> GetCityTiles()
    {
        List<Vector3Int> list = new List<Vector3Int>();

        Vector3Int cellPosition;
        TerrainTile tile;
        for (int y = CellBounds.yMin; y < CellBounds.yMax; y++)
        {
            for (int x = CellBounds.xMin; x < CellBounds.xMax; x++)
            {
                cellPosition = new Vector3Int(x,y,0);
                tile = _graphics.GetTile<TerrainTile>(cellPosition);

                if (tile != null && tile.terrain == Terrain.City)
                    list.Add(cellPosition);
            }
        }

        return list;
    }

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

    public bool TileIsSurrounded(Vector3Int cell) 
    {
        return _adjacencies.All(dir => {
            TileBase tile = _logic.GetTile(cell+dir);
            return tile == tileOccupation[TileType.Occupied];
        });
    }

    public bool IsAdjacentToHumans(Vector3Int cellPosition) 
    {
        return _adjacencies.Any(dir => 
        {
            Vector3Int adjacentCell = cellPosition+dir;
            if((adjacentCell).x < CellBounds.xMin || CellBounds.xMax < (adjacentCell).x) return false;
            if((adjacentCell).y < CellBounds.yMin || CellBounds.yMax < (adjacentCell).y) return false;

            TerrainTile adjacentTile = _graphics.GetTile<TerrainTile>(adjacentCell);
            if (adjacentTile != null)
                return humanTerrains.Contains(adjacentTile.terrain);
            else
                return false;
        }
        );
    }
    public bool IsAdjacentToMountains(Vector3Int cellPosition)
    {
        return _adjacencies.Any(dir => 
        {
            Vector3Int adjacentCell = cellPosition+dir;
            if((adjacentCell).x < CellBounds.xMin || CellBounds.xMax < (adjacentCell).x) return false;
            if((adjacentCell).y < CellBounds.yMin || CellBounds.yMax < (adjacentCell).y) return false;

            TerrainTile tile = _graphics.GetTile<TerrainTile>(adjacentCell);
            if (tile != null)
                return tile.terrain == Terrain.Mountain;
            else
                return false;
        }
        );
    }
    public bool IsAdjacentToWater(Vector3Int cellPosition)
    {
        return _adjacencies.Any(dir => 
        {
            Vector3Int adjacentCell = cellPosition+dir;
            if((adjacentCell).x < CellBounds.xMin || CellBounds.xMax < (adjacentCell).x) return false;
            if((adjacentCell).y < CellBounds.yMin || CellBounds.yMax < (adjacentCell).y) return false;

            TerrainTile tile = _graphics.GetTile<TerrainTile>(adjacentCell);
            if (tile != null)
                return tile.terrain == Terrain.Water;
            else
                return false;
        }
        );
    }
    public bool WithinRangeOfHumans(Vector3Int cellPosition)
    {
        bool within2Tiles = _range2Adjacencies.Any(dir => 
        {
            Vector3Int adjacentCell = cellPosition+dir;
            if((adjacentCell).x < CellBounds.xMin || CellBounds.xMax < (adjacentCell).x) return false;
            if((adjacentCell).y < CellBounds.yMin || CellBounds.yMax < (adjacentCell).y) return false;

            if (Mathf.Abs(dir.x) + Mathf.Abs(dir.y) == 2)
            {
                if (Mathf.Abs(dir.x) == 2 || Mathf.Abs(dir.y) == 2)
                {
                    TileBase middleTile = _logic.GetTile(cellPosition+(dir/2));
                    if (middleTile == tileOccupation[TileType.Occupied]) return false;
                }
                else
                {
                    Vector3Int[] cardinalCell = new Vector3Int[2];
                    cardinalCell[0] = cellPosition + new Vector3Int(dir.x,0,0);
                    cardinalCell[1] = cellPosition+ new Vector3Int(0,dir.y,0);

                    bool pathObstructed = cardinalCell.All(c => _logic.GetTile(c) == tileOccupation[TileType.Occupied]);
                    if (pathObstructed) return false;
                }
            } 

            TerrainTile adjacentTile = _graphics.GetTile<TerrainTile>(adjacentCell);

            if (adjacentTile != null)
                return humanTerrains.Contains(adjacentTile.terrain);
            else
                return false;
        }
        );

        return within2Tiles;
    }

    public bool Validate(Vector3Int cellPosition, TerrainTile tile)
    {
        Vector3 worldPos = _logic.CellToWorld(cellPosition);
        return Validate(worldPos, tile);
    }

    public bool Validate(Vector3 position, TerrainTile tile, bool bypassOccupancy=false) 
    {
        if (tile == null) return false;

        Vector3Int cellPosition = _logic.WorldToCell(position);

        bool tileIsUnclaimed = _logic.GetTile(cellPosition) == tileOccupation[TileType.Unclaimed];
        bool claimBypassed = bypassOccupancy && _logic.GetTile(cellPosition) != null;

        switch(tile.terrain)
        {
            #region Nature Tile Validations
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
            #endregion
            
            #region Human Tile Validations
                case Terrain.City:
                    return (tileIsUnclaimed || claimBypassed) && IsAdjacentToHumans(cellPosition);

                case Terrain.Mine:
                    return (tileIsUnclaimed || claimBypassed) && IsAdjacentToMountains(cellPosition) && WithinRangeOfHumans(cellPosition);

                case Terrain.Farm:
                    return (tileIsUnclaimed || claimBypassed) && IsAdjacentToWater(cellPosition) && WithinRangeOfHumans(cellPosition);
            #endregion

            default: return true;
        }
    }

    public void Submit(Vector3Int cellPosition, TerrainTile tile)
    {
        Vector3 worldPos = _logic.CellToWorld(cellPosition);
        Submit(worldPos, tile);
    }

    public void Submit(Vector3 position, TerrainTile tile, bool bypassOccupancy=false)
    {
        if (!Validate(position, tile, bypassOccupancy)) return;
        
        // print($"Placing {tile.terrain}");
        Vector3Int cellPosition = _logic.WorldToCell(position);
        TerrainTile oldTile = _graphics.GetTile<TerrainTile>(cellPosition);

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
            case Terrain.Forest:
            case Terrain.Water:
                NatureScore++;
                OnNatureScoreChanged?.Invoke(NatureScore);
                break;

            case Terrain.Mountain:
                humanTiles = new Terrain[] {Terrain.City, Terrain.Mine, Terrain.Farm};
                foreach (Vector3Int dir in _adjacencies)
                {
                    currentTile = _graphics.GetTile<TerrainTile>(cellPosition+dir);
                    
                    if (currentTile != null && humanTiles.Contains(currentTile.terrain))
                    {
                        Submit(_logic.CellToWorld(cellPosition+dir), tileBases[Terrain.Plains], true);
                    }    
                }
                NatureScore++;
                OnNatureScoreChanged?.Invoke(NatureScore);
                break;

            case Terrain.City:
                foreach (Vector3Int dir in _adjacencies)
                {
                    currentTile = _graphics.GetTile<TerrainTile>(cellPosition+dir);
                    if (currentTile != null && currentTile.terrain == Terrain.Forest)
                    {
                        int adjacentCities = _adjacencies.Count(dir_f => 
                            _graphics.GetTile<TerrainTile>(cellPosition+dir+dir_f)?.terrain == Terrain.City
                        );

                        if (adjacentCities >= 2)
                            Submit(_logic.CellToWorld(cellPosition+dir), tileBases[Terrain.City], true);
                    }
                }
                HumanScore++;
                OnHumansScoreChanged?.Invoke(HumanScore);
                break;

            case Terrain.Mine:
            case Terrain.Farm:
                HumanMovesPerTurn++;
                HumanScore++;
                OnHumansScoreChanged?.Invoke(HumanScore);
                break;

            case Terrain.Plains:
                humanTiles = new Terrain[] {Terrain.Mine, Terrain.Farm};
                
                if (humanTiles.Contains(oldTile.terrain)) 
                {
                    HumanMovesPerTurn = Mathf.Max(2, HumanMovesPerTurn-1);
                    HumanScore--;
                    OnHumansScoreChanged?.Invoke(HumanScore);
                }
                break;

            default: break;
        }

        print($"Unclaimed tiles remaining: {UnclaimedTiles}");
        OnResourceAdded?.Invoke(cellPosition, tile);

        if (UnclaimedTiles <= 0) GameManager.Instance.EndGame();
    }
}