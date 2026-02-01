using Avalonia.Input;

namespace AVARace.Services.Interfaces;

public interface IInputHandler
{
    bool IsRotatingLeft { get; }
    bool IsRotatingRight { get; }
    bool IsThrusting { get; }
    bool IsFiring { get; }

    void HandleKeyDown(Key key);
    void HandleKeyUp(Key key);
    void Reset();

    event Action? OnPausePressed;
    event Action? OnRestartPressed;
    event Action? OnExitPressed;
}
