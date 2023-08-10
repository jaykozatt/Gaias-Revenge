using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyAI : StaticInstance<EnemyAI>
{
    private Vector3Int[] _adjacencies;
    private Vector3Int[] _range2Adjacencies;

    private List<Vector3Int> _frontierTiles;
    private List<Vector3Int> _cityReachables;
    private List<Vector3Int> _mineReachables;
    private List<Vector3Int> _farmReachables;

    private int _movesLeft;

    int PossibleMoves {get=> _cityReachables.Count + _mineReachables.Count + _farmReachables.Count;}

    Coroutine aiRoutine;

    IEnumerator AIRoutine()
    {
        while (GameManager.Instance.gameState < GameState.GameEnded)
        {
            while (GameManager.Instance.gameState < GameState.TurnCPU)
            {
                yield return null;
            }

            if (GameManager.Instance.gameState == GameState.GameEnded) break;

            _movesLeft = GameBoard.Instance.HumanMovesPerTurn;
            ExploreAndCullFrontier();

            while (_movesLeft > 0)
            {
                if (TryPlaceTile(Terrain.Farm) || TryPlaceTile(Terrain.Mine) || TryPlaceTile(Terrain.City)) 
                    _movesLeft--;
                else
                {
                    break;
                }
                yield return null;
            }

            if (GameManager.Instance.gameState != GameState.GameEnded)
                GameManager.Instance.AdvanceTurn();

            yield return null;
        }
    }

    protected override void Awake() {
        _adjacencies = new Vector3Int[] {
            Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left 
        };

        _range2Adjacencies = new Vector3Int[] {
            Vector3Int.up + Vector3Int.right, Vector3Int.down + Vector3Int.left,
            Vector3Int.up - Vector3Int.right, Vector3Int.down - Vector3Int.left,
            2*Vector3Int.up, 2*Vector3Int.right, 2*Vector3Int.down, 2*Vector3Int.left
        };
    }

    private void OnDestroy() {
        if (aiRoutine != null) StopCoroutine(aiRoutine);
    }

    // Start is called before the first frame update
    void Start()
    {
        GameBoard.Instance.OnResourceAdded += OnResourceAdded;
        GameManager.Instance.OnGameStart += Initialise;
    }

    void Initialise()
    {
        _cityReachables = new List<Vector3Int>();
        _mineReachables = new List<Vector3Int>();
        _farmReachables = new List<Vector3Int>();

        _frontierTiles = GameBoard.Instance.GetCityTiles();
        
        ExploreAndCullFrontier();
        print($"[AI] Possible Moves: {PossibleMoves}");

        aiRoutine = StartCoroutine(AIRoutine());
    } 

    

    bool TryPlaceTile(Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.Mine:
                if (_mineReachables.Count > 0)
                {
                    GameBoard.Instance.Submit(
                        _mineReachables[Random.Range(0,_mineReachables.Count)], 
                        GameBoard.tileBases[terrain]
                    );
                    return true;
                }
                else
                {
                    return false;
                }

            case Terrain.Farm:
                if (_farmReachables.Count > 0)
                {
                    GameBoard.Instance.Submit(
                        _farmReachables[Random.Range(0,_farmReachables.Count)], 
                        GameBoard.tileBases[terrain]
                    );
                    return true;
                }
                else
                {
                    return false;
                }

            case Terrain.City:
                if (_cityReachables.Count > 0)
                {
                    GameBoard.Instance.Submit(
                        _cityReachables[Random.Range(0,_cityReachables.Count)], 
                        GameBoard.tileBases[terrain]
                    );
                    return true;
                }
                else
                {
                    return false;
                }

            default: return false;
        }
    }

    void OnResourceAdded(Vector3Int cellPosition, TerrainTile tile)
    {
        _cityReachables.Remove(cellPosition);
        _mineReachables.Remove(cellPosition);
        _farmReachables.Remove(cellPosition);

        Vector3Int probe;

        bool isFarmValid;
        bool isMineValid;
        bool isCityValid;

        switch (tile.terrain)
        {
            case Terrain.Mountain:
                foreach (Vector3Int dir in _adjacencies)
                {   
                    probe = cellPosition+dir;
                    isMineValid = GameBoard.Instance.Validate(probe, GameBoard.tileBases[Terrain.Mine]);

                    if (!_mineReachables.Contains(probe) && isMineValid)
                        _mineReachables.Add(probe);
                }
                break;

            case Terrain.Water:
                foreach (Vector3Int dir in _adjacencies)
                {   
                    probe = cellPosition+dir;
                    isFarmValid = GameBoard.Instance.Validate(probe, GameBoard.tileBases[Terrain.Farm]);

                    if (!_farmReachables.Contains(probe) && isFarmValid)
                        _farmReachables.Add(probe);
                }
                break;
            
            case Terrain.Farm:
            case Terrain.Mine:
            case Terrain.City:
                if (!_frontierTiles.Contains(cellPosition)) _frontierTiles.Add(cellPosition);
                foreach (Vector3Int dir in _adjacencies)
                {
                    probe = cellPosition+dir;
                    isCityValid = GameBoard.Instance.Validate(probe, GameBoard.tileBases[Terrain.City]);
                    if (!_cityReachables.Contains(probe) && isCityValid)
                    {
                        _cityReachables.Add(probe);
                    }
                }
                break;

            default:
                isFarmValid = GameBoard.Instance.Validate(cellPosition, GameBoard.tileBases[Terrain.Farm]);
                isMineValid = GameBoard.Instance.Validate(cellPosition, GameBoard.tileBases[Terrain.Mine]);
                isCityValid = GameBoard.Instance.Validate(cellPosition, GameBoard.tileBases[Terrain.City]);
                
                if (!_farmReachables.Contains(cellPosition) && isFarmValid)
                    _farmReachables.Add(cellPosition);

                if (!_mineReachables.Contains(cellPosition) && isMineValid)
                    _mineReachables.Add(cellPosition);

                if (!_cityReachables.Contains(cellPosition) && isCityValid)
                    _cityReachables.Add(cellPosition);

                break;
        }

        ExploreAndCullFrontier();
        print($"[AI] Possible Moves: {PossibleMoves}");
    }

    void ExploreAndCullFrontier() 
    {
        Vector3Int prospectCell;
        Vector3Int tile;
        for (int i = 0; i < _frontierTiles.Count; i++)
        {
            tile = _frontierTiles[i];
            if (GameBoard.Instance.TileIsSurrounded(tile))
            {
                print ($"Removed tile: {tile}");
                _frontierTiles.Remove(tile);
                i--;
                continue;
            }

            // Explore adjacent tiles to each city
            foreach (Vector3Int dir in _adjacencies)
            {
                prospectCell = tile+dir;
                if (GameBoard.Instance.Validate(prospectCell, GameBoard.tileBases[Terrain.City]))
                {
                    if (!_cityReachables.Contains(prospectCell))
                        _cityReachables.Add(prospectCell);
                }
                
                if (GameBoard.Instance.Validate(prospectCell, GameBoard.tileBases[Terrain.Mine]))
                {
                    if (!_mineReachables.Contains(prospectCell))
                        _mineReachables.Add(prospectCell);
                }

                if (GameBoard.Instance.Validate(prospectCell, GameBoard.tileBases[Terrain.Farm]))
                {
                    if (!_farmReachables.Contains(prospectCell))
                        _farmReachables.Add(prospectCell);
                }
            }

            foreach (Vector3Int dir in _range2Adjacencies)
            {
                prospectCell = tile+dir;
                if (GameBoard.Instance.Validate(prospectCell, GameBoard.tileBases[Terrain.Mine]))
                {
                    if (!_mineReachables.Contains(prospectCell))
                        _mineReachables.Add(prospectCell);
                }

                if (GameBoard.Instance.Validate(prospectCell, GameBoard.tileBases[Terrain.Farm]))
                {
                    if (!_farmReachables.Contains(prospectCell))
                        _farmReachables.Add(prospectCell);
                }
            }
        }

        // if (_cityReachables == null || _mineReachables != null)
        // {
            
        // }
        // else 
        // {
        //     foreach (Vector3Int tile in _cityReachables)
        //     {
        //         foreach (Vector3Int dir in _adjacencies)
        //         {
        //             if (GameBoard.Instance.Validate(tile+dir, GameBoard.tileBases[Terrain.Mine]))
        //             {
        //                 if (!_mineReachables.Contains(tile+dir))
        //                 _mineReachables.Add(tile+dir);
        //             }

        //             if (GameBoard.Instance.Validate(tile+dir, GameBoard.tileBases[Terrain.Farm]))
        //             {
        //                 if (!_farmReachables.Contains(tile+dir))
        //                 _farmReachables.Add(tile+dir);
        //             }
        //         }
        //     }
        // }
    }


}
