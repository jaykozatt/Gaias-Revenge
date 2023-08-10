using UnityEngine;
using UnityEngine.Tilemaps;

public class Placeable : Dragable 
{
    Tilemap _tilemap;
    BoundsInt _cellBounds;
    Vector3 _originalPosition;
    Dispenser _dispenser;

    public void AssignDeployer(Dispenser dispenser) 
    {
        _dispenser = dispenser;
    }

    private void OnMouseEnter() {
        if (enabled && GameManager.Instance.gameState == GameState.TurnPlayer)
        {
            Vector3Int cellPosition;
            for (int y = _cellBounds.yMin; y < _cellBounds.yMax; y++)
            {
                for (int x = _cellBounds.xMin; x < _cellBounds.xMax; x++)
                {
                    cellPosition = new Vector3Int(x,y,0);
                    _tilemap.SetColor(cellPosition, Color.yellow);   
                }
            }
        }

    }
    private void OnMouseExit() {
        if (enabled && GameManager.Instance.gameState == GameState.TurnPlayer)
        {
            Vector3Int cellPosition;
            for (int y = _cellBounds.yMin; y < _cellBounds.yMax; y++)
            {
                for (int x = _cellBounds.xMin; x < _cellBounds.xMax; x++)
                {
                    cellPosition = new Vector3Int(x,y,0);
                    _tilemap.SetColor(cellPosition, Color.white);   
                }
            }
        }
    }

    protected override void OnMouseDown()
    {
        if (enabled && GameManager.Instance.gameState == GameState.TurnPlayer)
        {
            base.OnMouseDown();
            _originalPosition = transform.position;
            _tilemap.CompressBounds();
            _tilemap.tileAnchor += .75f * Vector3.forward;
            GameBoard.Instance.EnableHelperGrid();

            Vector3Int cellPosition;
            for (int y = _cellBounds.yMin; y < _cellBounds.yMax; y++)
            {
                for (int x = _cellBounds.xMin; x < _cellBounds.xMax; x++)
                {
                    cellPosition = new Vector3Int(x,y,0);
                    _tilemap.SetColor(cellPosition, Color.white);   
                }
            }
        }
    }

    protected override void OnMouseDrag() 
    {
        if (enabled && GameManager.Instance.gameState == GameState.TurnPlayer)
        {
            base.OnMouseDrag();    
            GameBoard.Instance.ClearTemp();

            TerrainTile tile;
            Vector3 tileWorldPosition;
            Vector3Int cellPosition;
            for (int y = _cellBounds.yMin; y < _cellBounds.yMax; y++)
            {
                for (int x = _cellBounds.xMin; x < _cellBounds.xMax; x++)
                {
                    cellPosition = new Vector3Int(x,y,0);
                    tileWorldPosition = _tilemap.CellToWorld(cellPosition);

                    tile = _tilemap.GetTile<TerrainTile>(cellPosition);
                    
                    if (GameBoard.Instance.Validate(tileWorldPosition, tile))
                        _tilemap.SetColor(cellPosition, Color.green);
                    else
                        _tilemap.SetColor(cellPosition, Color.red);   
                }
            }
        }
    }

    private void OnMouseUp() 
    {
        if (enabled && GameManager.Instance.gameState == GameState.TurnPlayer)
        {
            Vector3 tileWorldPosition;
            Vector3Int cellPosition;
            TerrainTile tile;
            bool anyTileWasPlaced = false;

            for (int y = _cellBounds.yMin; y < _cellBounds.yMax; y++)
            {
                for (int x = _cellBounds.xMin; x < _cellBounds.xMax; x++)
                {
                    cellPosition = new Vector3Int(x,y,0);
                    tileWorldPosition = _tilemap.CellToWorld(cellPosition);
                    tile = _tilemap.GetTile<TerrainTile>(cellPosition);

                    if (tile != null && GameBoard.Instance.Validate(tileWorldPosition,tile))
                    {
                        GameBoard.Instance.Submit(tileWorldPosition, tile);  
                        anyTileWasPlaced = true;
                    }
                    else
                    {
                        _tilemap.SetColor(cellPosition, Color.white);
                    }

                    GameBoard.Instance.ClearTemp();
                }
            }

            GameBoard.Instance.EnableHelperGrid(false);
            _tilemap.tileAnchor -= .75f * Vector3.forward;

            if (anyTileWasPlaced) 
            {
                _dispenser?.DeployNext();
                Destroy(gameObject);

                if (GameBoard.Instance.UnclaimedTiles > 0 && GameManager.Instance.gameState < GameState.GameEnded)
                    GameManager.Instance.AdvanceTurn();
            }
            else 
            {
                transform.position = _originalPosition;
            }
        }
    }    

    private void Start() {
        _tilemap = GetComponentInChildren<Tilemap>();
        _cellBounds = _tilemap.cellBounds;
    }
}