using UnityEngine;

public enum GameState {
    Init, TurnPlayer, TurnCPU, GameEnded
}

public class GameManager : StaticInstance<GameManager> 
{
    public GameState gameState = GameState.Init;

    protected override void Awake() {
        base.Awake();
    }

    private void Start() {
        
    }

}