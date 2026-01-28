using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AVARace.Game;
using AVARace.Services.Interfaces;

namespace AVARace.Views;

public class GameCanvas : Control
{
    private readonly DispatcherTimer _gameTimer;
    private DateTime _lastUpdate;

    public static readonly StyledProperty<IGameEngine?> GameEngineProperty =
        AvaloniaProperty.Register<GameCanvas, IGameEngine?>(nameof(GameEngine));

    public static readonly StyledProperty<GameRenderer?> RendererProperty =
        AvaloniaProperty.Register<GameCanvas, GameRenderer?>(nameof(Renderer));

    public IGameEngine? GameEngine
    {
        get => GetValue(GameEngineProperty);
        set => SetValue(GameEngineProperty, value);
    }

    public GameRenderer? Renderer
    {
        get => GetValue(RendererProperty);
        set => SetValue(RendererProperty, value);
    }

    public GameCanvas()
    {
        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
        };
        _gameTimer.Tick += OnGameTick;
        _lastUpdate = DateTime.Now;

        ClipToBounds = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _lastUpdate = DateTime.Now;
        _gameTimer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _gameTimer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        deltaTime = Math.Min(deltaTime, 0.1);

        GameEngine?.Update(deltaTime);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        Renderer?.Render(context, Bounds.Size);
    }
}
