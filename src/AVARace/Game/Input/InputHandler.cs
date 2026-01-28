using Avalonia.Input;
using AVARace.Services.Interfaces;

namespace AVARace.Game.Input;

public class InputHandler : IInputHandler
{
    private readonly IControllerService _controller;
    private readonly HashSet<Key> _pressedKeys = new();

    public bool IsRotatingLeft =>
        _pressedKeys.Contains(Key.Left) ||
        _pressedKeys.Contains(Key.A) ||
        _controller.LeftStickX < -0.3f ||
        _controller.IsDPadLeft;

    public bool IsRotatingRight =>
        _pressedKeys.Contains(Key.Right) ||
        _pressedKeys.Contains(Key.D) ||
        _controller.LeftStickX > 0.3f ||
        _controller.IsDPadRight;

    public bool IsThrusting =>
        _pressedKeys.Contains(Key.Up) ||
        _pressedKeys.Contains(Key.W) ||
        _controller.IsButtonAPressed ||
        _controller.RightTrigger > 0.3f ||
        _controller.IsDPadUp;

    public bool IsFiring =>
        _pressedKeys.Contains(Key.Space) ||
        _controller.IsButtonXPressed ||
        _controller.IsButtonBPressed ||
        _controller.LeftTrigger > 0.3f;

    public event Action? OnPausePressed;
    public event Action? OnRestartPressed;
    public event Action? OnExitPressed;

    public InputHandler(IControllerService controller)
    {
        _controller = controller;
        _controller.Initialize();

        _controller.OnStartPressed += () => OnPausePressed?.Invoke();
        _controller.OnBackPressed += () => OnRestartPressed?.Invoke();
    }

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

    public void UpdateController()
    {
        _controller.Update();
    }
}
