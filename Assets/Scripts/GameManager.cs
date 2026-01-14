using UnityEngine;

public enum GameState { Playing, Popping, GameOver }

public class GameManager : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private UIManager uiManager;
    
    [Header("Timer")]
    [SerializeField] private float maxTime = 60f;
    private float currentTime;
    private int lastTime;
    private bool isTimerRunning;
    
    private GameState currentState;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += SetState;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= SetState;
    }

    private void Start()
    {
        SetState(GameState.Playing);
        GameStart();
    }

    private void Update()
    {
        if (!isTimerRunning || currentState == GameState.GameOver) return;

        currentTime -= Time.deltaTime;
        int floorTime = Mathf.FloorToInt(currentTime);

        if (floorTime != lastTime)
        {
            lastTime = floorTime;
            GameEvents.RaiseTimeChanged(floorTime);
        }

        if (currentTime > 0) return;
        gridManager.autoMod = false;
        if(currentState != GameState.Popping) GameOver();
    }

    private void SetState(GameState newState)
    {
        currentState = newState;
    }

    private void GameStart()
    {
        currentTime = maxTime;
        lastTime = (int)maxTime;
        isTimerRunning = true;
    }

    private void GameOver()
    {
        SetState(GameState.GameOver);
        uiManager.GameOver(gridManager.Score);
    }

    public void Restart()
    {
        uiManager.SetGameOverUIActive(false);
        gridManager.ResetGrid();
        GameStart();
    }
}
