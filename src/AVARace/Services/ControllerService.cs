using AVARace.Services.Interfaces;
using Silk.NET.SDL;

namespace AVARace.Services;

public unsafe class ControllerService : IControllerService
{
    private Sdl? _sdl;
    private GameController* _controller;
    private bool _isDisposed;

    private bool _wasStartPressed;
    private bool _wasBackPressed;

    private const float DeadZone = 0.15f;

    public bool IsConnected => _controller != null;

    public float LeftStickX { get; private set; }
    public float LeftStickY { get; private set; }
    public float RightTrigger { get; private set; }
    public float LeftTrigger { get; private set; }

    public bool IsButtonAPressed { get; private set; }
    public bool IsButtonBPressed { get; private set; }
    public bool IsButtonXPressed { get; private set; }
    public bool IsButtonYPressed { get; private set; }
    public bool IsStartPressed { get; private set; }
    public bool IsBackPressed { get; private set; }
    public bool IsDPadLeft { get; private set; }
    public bool IsDPadRight { get; private set; }
    public bool IsDPadUp { get; private set; }
    public bool IsDPadDown { get; private set; }

    public event Action? OnStartPressed;
    public event Action? OnBackPressed;

    public void Initialize()
    {
        try
        {
            _sdl = Sdl.GetApi();
            _sdl.Init(Sdl.InitGamecontroller | Sdl.InitJoystick);
            TryConnectController();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller init failed: {ex.Message}");
        }
    }

    private void TryConnectController()
    {
        if (_sdl == null) return;

        var numJoysticks = _sdl.NumJoysticks();
        for (int i = 0; i < numJoysticks; i++)
        {
            if (_sdl.IsGameController(i) == SdlBool.True)
            {
                _controller = _sdl.GameControllerOpen(i);
                if (_controller != null)
                {
                    Console.WriteLine($"Controller connected: {new string((sbyte*)_sdl.GameControllerName(_controller))}");
                    break;
                }
            }
        }
    }

    public void Update()
    {
        if (_sdl == null) return;

        _sdl.PumpEvents();

        if (_controller == null)
        {
            TryConnectController();
            return;
        }

        if (_sdl.GameControllerGetAttached(_controller) == SdlBool.False)
        {
            _sdl.GameControllerClose(_controller);
            _controller = null;
            Console.WriteLine("Controller disconnected");
            return;
        }

        // Left stick
        var rawX = _sdl.GameControllerGetAxis(_controller, GameControllerAxis.Leftx) / 32767.0f;
        var rawY = _sdl.GameControllerGetAxis(_controller, GameControllerAxis.Lefty) / 32767.0f;
        LeftStickX = ApplyDeadzone(rawX);
        LeftStickY = ApplyDeadzone(rawY);

        // Triggers
        RightTrigger = _sdl.GameControllerGetAxis(_controller, GameControllerAxis.Triggerright) / 32767.0f;
        LeftTrigger = _sdl.GameControllerGetAxis(_controller, GameControllerAxis.Triggerleft) / 32767.0f;

        // Buttons
        IsButtonAPressed = _sdl.GameControllerGetButton(_controller, GameControllerButton.A) == 1;
        IsButtonBPressed = _sdl.GameControllerGetButton(_controller, GameControllerButton.B) == 1;
        IsButtonXPressed = _sdl.GameControllerGetButton(_controller, GameControllerButton.X) == 1;
        IsButtonYPressed = _sdl.GameControllerGetButton(_controller, GameControllerButton.Y) == 1;

        var startPressed = _sdl.GameControllerGetButton(_controller, GameControllerButton.Start) == 1;
        var backPressed = _sdl.GameControllerGetButton(_controller, GameControllerButton.Back) == 1;

        // D-Pad
        IsDPadLeft = _sdl.GameControllerGetButton(_controller, GameControllerButton.DpadLeft) == 1;
        IsDPadRight = _sdl.GameControllerGetButton(_controller, GameControllerButton.DpadRight) == 1;
        IsDPadUp = _sdl.GameControllerGetButton(_controller, GameControllerButton.DpadUp) == 1;
        IsDPadDown = _sdl.GameControllerGetButton(_controller, GameControllerButton.DpadDown) == 1;

        // Fire events on button press (not hold)
        if (startPressed && !_wasStartPressed)
        {
            OnStartPressed?.Invoke();
        }
        if (backPressed && !_wasBackPressed)
        {
            OnBackPressed?.Invoke();
        }

        IsStartPressed = startPressed;
        IsBackPressed = backPressed;
        _wasStartPressed = startPressed;
        _wasBackPressed = backPressed;
    }

    private static float ApplyDeadzone(float value)
    {
        if (Math.Abs(value) < DeadZone)
            return 0f;

        var sign = Math.Sign(value);
        var adjusted = (Math.Abs(value) - DeadZone) / (1f - DeadZone);
        return sign * (float)adjusted;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_controller != null && _sdl != null)
        {
            _sdl.GameControllerClose(_controller);
            _controller = null;
        }

        _sdl?.Quit();
    }
}
