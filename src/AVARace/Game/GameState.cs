using CommunityToolkit.Mvvm.ComponentModel;

namespace AVARace.Game;

public partial class GameState : ObservableObject
{
    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _lives = 3;

    [ObservableProperty]
    private int _wave = 1;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isGameOver;

    public void Reset()
    {
        Score = 0;
        Lives = 3;
        Wave = 1;
        IsRunning = false;
        IsPaused = false;
        IsGameOver = false;
    }

    public void AddScore(int points)
    {
        Score += points;
    }

    public void LoseLife()
    {
        Lives--;
        if (Lives <= 0)
        {
            IsGameOver = true;
            IsRunning = false;
        }
    }

    public void NextWave()
    {
        Wave++;
    }
}
