using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {
    Init, TurnPlayer, TurnCPU, GameEnded
}

public class GameManager : StaticInstance<GameManager> 
{
    public GameState gameState = GameState.Init;
    public GameObject startPanel;
    public GameObject endPanel;
    public Action OnGameStart;
    public Action OnGameEnded;

    protected override void Awake() {
        Application.targetFrameRate = 30;
        base.Awake();
    }

    private void Start() {
        // StartGame();
    }

    public void StartGame() 
    {
        gameState = GameState.TurnPlayer;
        startPanel.SetActive(false);
        OnGameStart?.Invoke();
    }

    public void AdvanceTurn()
    {
        gameState = gameState == GameState.TurnPlayer? GameState.TurnCPU : GameState.TurnPlayer;
    }

    public void EndGame()
    {
        gameState = GameState.GameEnded;
        endPanel.SetActive(true);
        OnGameEnded?.Invoke();
    }

    public void Reset()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit() 
    {
        Application.Quit();
    }

}