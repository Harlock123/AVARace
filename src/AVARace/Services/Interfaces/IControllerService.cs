namespace AVARace.Services.Interfaces;

public interface IControllerService : IDisposable
{
    bool IsConnected { get; }

    float LeftStickX { get; }
    float LeftStickY { get; }
    float RightTrigger { get; }
    float LeftTrigger { get; }

    bool IsButtonAPressed { get; }
    bool IsButtonBPressed { get; }
    bool IsButtonXPressed { get; }
    bool IsButtonYPressed { get; }
    bool IsStartPressed { get; }
    bool IsBackPressed { get; }
    bool IsDPadLeft { get; }
    bool IsDPadRight { get; }
    bool IsDPadUp { get; }
    bool IsDPadDown { get; }

    void Initialize();
    void Update();

    event Action? OnStartPressed;
    event Action? OnBackPressed;
}
