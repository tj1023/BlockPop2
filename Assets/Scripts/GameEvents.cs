using System;

public static class GameEvents
{
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnTimeChanged;
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<Block, Block> OnBlockSwapRequested;

    
    public static void RaiseScoreChanged(int score)
    {
        OnScoreChanged?.Invoke(score);
    }
    
    public static void RaiseTimeChanged(int time)
    {
        OnTimeChanged?.Invoke(time);
    }
    
    public static void RaiseGameStateChanged(GameState state)
    {
        OnGameStateChanged?.Invoke(state);
    }
    
    public static void RaiseBlockSwapRequested(Block a, Block b)
    {
        OnBlockSwapRequested?.Invoke(a, b);
    }
}
