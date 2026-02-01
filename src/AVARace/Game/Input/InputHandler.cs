using Avalonia.Input;
using AVARace.Services.Interfaces;

namespace AVARace.Game.Input;

public class InputHandler : IInputHandler
{
    private readonly HashSet<Key> _pressedKeys = new();

    public bool IsRotatingLeft =>
        _pressedKeys.Contains(Key.Left) ||
        _pressedKeys.Contains(Key.A);

    public bool IsRotatingRight =>
        _pressedKeys.Contains(Key.Right) ||
        _pressedKeys.Contains(Key.D);

    public bool IsThrusting =>
        _pressedKeys.Contains(Key.Up) ||
        _pressedKeys.Contains(Key.W);

    public bool IsFiring =>
        _pressedKeys.Contains(Key.Space);

    public event Action? OnPausePressed;
    public event Action? OnRestartPressed;
    public event Action? OnExitPressed;

    public void HandleKeyDown(Key key)
    {
        _pressedKeys.Add(key);

        switch (key)
        {
            case Key.P:
                OnPausePressed?.Invoke();
                break;
            case Key.R:
                OnRestartPressed?.Invoke();
                break;
            case Key.Escape:
                OnExitPressed?.Invoke();
                break;
        }
    }

    public void HandleKeyUp(Key key)
    {
        _pressedKeys.Remove(key);
    }

    public void Reset()
    {
        _pressedKeys.Clear();
    }
}
