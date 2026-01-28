using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AVARace.Services.Interfaces;
using AVARace.Game;

namespace AVARace.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IGameEngine _gameEngine;
    private readonly IInputHandler _inputHandler;
    private readonly GameRenderer _renderer;

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _lives;

    [ObservableProperty]
    private int _wave;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isGameOver;

    [ObservableProperty]
    private string _statusMessage = "Press SPACE to start";

    public IGameEngine GameEngine => _gameEngine;
    public IInputHandler InputHandler => _inputHandler;
    public GameRenderer Renderer => _renderer;

    public MainWindowViewModel(IGameEngine gameEngine, IInputHandler inputHandler, GameRenderer renderer)
    {
        _gameEngine = gameEngine;
        _inputHandler = inputHandler;
        _renderer = renderer;

        _gameEngine.OnGameUpdated += OnGameUpdated;
        _gameEngine.OnGameOver += OnGameOver;
        _inputHandler.OnPausePressed += TogglePause;
        _inputHandler.OnRestartPressed += RestartGame;

        UpdateFromGameState();
    }

    private void OnGameUpdated()
    {
        UpdateFromGameState();
    }

    private void UpdateFromGameState()
    {
        Score = _gameEngine.State.Score;
        Lives = _gameEngine.State.Lives;
        Wave = _gameEngine.State.Wave;
        IsPaused = _gameEngine.State.IsPaused;
        IsGameOver = _gameEngine.State.IsGameOver;

        if (IsGameOver)
        {
            StatusMessage = $"GAME OVER - Score: {Score} - Press R to restart";
        }
        else if (IsPaused)
        {
            StatusMessage = "PAUSED - Press P to continue";
        }
        else if (!_gameEngine.State.IsRunning)
        {
            StatusMessage = "Press SPACE to start";
        }
        else
        {
            StatusMessage = string.Empty;
        }
    }

    private void OnGameOver()
    {
        IsGameOver = true;
        StatusMessage = $"GAME OVER - Score: {Score} - Press R to restart";
    }

    [RelayCommand]
    private void StartGame()
    {
        if (!_gameEngine.State.IsRunning)
        {
            _gameEngine.Start();
        }
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (_gameEngine.State.IsGameOver) return;

        if (_gameEngine.State.IsPaused)
        {
            _gameEngine.Resume();
        }
        else
        {
            _gameEngine.Pause();
        }
    }

    [RelayCommand]
    private void RestartGame()
    {
        _gameEngine.Reset();
        _gameEngine.Start();
    }
}
