using Avalonia;
using Avalonia.Media;
using AVARace.Game.Entities;
using AVARace.Services.Interfaces;

namespace AVARace.Game;

public class GameRenderer
{
    private readonly IGameEngine _gameEngine;

    private static readonly Color VectorGreen = Color.FromRgb(0, 255, 0);
    private static readonly Color DimGreen = Color.FromRgb(0, 180, 0);
    private static readonly Color BrightGreen = Color.FromRgb(100, 255, 100);

    private readonly Pen _arenaPen;
    private readonly Pen _entityPen;
    private readonly Pen _bulletPen;
    private readonly Pen _playerPen;
    private readonly SolidColorBrush _textBrush;
    private readonly SolidColorBrush _whiteTextBrush;

    public GameRenderer(IGameEngine gameEngine)
    {
        _gameEngine = gameEngine;

        _arenaPen = new Pen(new SolidColorBrush(DimGreen), 2);
        _entityPen = new Pen(new SolidColorBrush(VectorGreen), 2);
        _bulletPen = new Pen(new SolidColorBrush(BrightGreen), 2);
        _playerPen = new Pen(new SolidColorBrush(VectorGreen), 2.5);
        _textBrush = new SolidColorBrush(VectorGreen);
        _whiteTextBrush = new SolidColorBrush(Colors.White);
    }

    public void Render(DrawingContext context, Size size)
    {
        context.FillRectangle(Brushes.Black, new Rect(size));

        if (_gameEngine is GameEngine engine)
        {
            engine.SetArenaSize(size.Width, size.Height);
        }

        RenderArena(context);

        if (!_gameEngine.State.IsRunning && !_gameEngine.State.IsGameOver)
        {
            RenderStartScreen(context, size);
        }
        else
        {
            RenderEntities(context);
            RenderHUD(context, size);
        }
    }

    private void RenderArena(DrawingContext context)
    {
        var arena = _gameEngine.Arena;

        DrawPolygon(context, arena.OuterBoundary, _arenaPen);
        DrawPolygon(context, arena.CentralObstacle, _arenaPen);
    }

    private void RenderEntities(DrawingContext context)
    {
        foreach (var entity in _gameEngine.Entities)
        {
            if (!entity.IsAlive) continue;

            if (entity is Explosion explosion)
            {
                DrawExplosion(context, explosion);
                continue;
            }

            // Skip drawing invulnerable player during blink-off phase
            if (entity is PlayerShip playerShip && playerShip.IsInvulnerable)
            {
                // Blink effect: visible for 100ms, invisible for 100ms
                var blinkOn = ((int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 100)) % 2 == 0;
                if (!blinkOn)
                {
                    continue;
                }
            }

            Pen pen = entity switch
            {
                PlayerShip => _playerPen,
                Bullet => _bulletPen,
                _ => _entityPen
            };

            var vertices = entity.GetTransformedVertices();
            DrawPolygon(context, vertices, pen);

            if (entity is PlayerShip ship)
            {
                DrawThrustFlame(context, ship);
            }
        }
    }

    private static readonly Color[] ExplosionColors = new[]
    {
        Color.FromRgb(255, 255, 100),  // Yellow
        Color.FromRgb(255, 200, 50),   // Gold
        Color.FromRgb(255, 150, 0),    // Orange
        Color.FromRgb(255, 100, 0),    // Dark Orange
        Color.FromRgb(255, 50, 50),    // Red-Orange
        Color.FromRgb(255, 255, 255),  // White (hot center)
        Color.FromRgb(100, 255, 100),  // Green (for variety)
        Color.FromRgb(255, 180, 180),  // Pink
    };

    private void DrawExplosion(DrawingContext context, Explosion explosion)
    {
        // Fade out as the explosion progresses
        var alpha = (byte)(255 * (1 - explosion.Progress));

        var particleIndex = 0;
        foreach (var particle in explosion.Particles)
        {
            // Each particle gets a different color from the palette
            var baseColor = ExplosionColors[particleIndex % ExplosionColors.Length];
            var color = Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
            var pen = new Pen(new SolidColorBrush(color), 2);

            context.DrawLine(pen, particle.StartPoint, particle.EndPoint);
            particleIndex++;
        }
    }

    private void DrawThrustFlame(DrawingContext context, PlayerShip ship)
    {
        var cos = Math.Cos(ship.Rotation);
        var sin = Math.Sin(ship.Rotation);

        var flameLength = 10 + new Random().NextDouble() * 5;
        var flameBase1 = new Point(
            -8 * cos - 4 * sin + ship.Position.X,
            -8 * sin + 4 * cos + ship.Position.Y
        );
        var flameBase2 = new Point(
            -8 * cos + 4 * sin + ship.Position.X,
            -8 * sin - 4 * cos + ship.Position.Y
        );
        var flameTip = new Point(
            (-8 - flameLength) * cos + ship.Position.X,
            (-8 - flameLength) * sin + ship.Position.Y
        );

        if (ship.Velocity.Length > 10)
        {
            context.DrawLine(_entityPen, flameBase1, flameTip);
            context.DrawLine(_entityPen, flameBase2, flameTip);
        }
    }

    private void DrawPolygon(DrawingContext context, Point[] vertices, Pen pen)
    {
        if (vertices.Length < 2) return;

        for (int i = 0; i < vertices.Length; i++)
        {
            var start = vertices[i];
            var end = vertices[(i + 1) % vertices.Length];
            context.DrawLine(pen, start, end);
        }
    }

    private void RenderHUD(DrawingContext context, Size size)
    {
        var state = _gameEngine.State;
        var typeface = new Typeface("Courier New", FontStyle.Normal, FontWeight.Bold);

        var scoreText = new FormattedText(
            $"SCORE: {state.Score:D6}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            20,
            _textBrush
        );
        context.DrawText(scoreText, new Point(20, 20));

        var livesText = new FormattedText(
            $"LIVES: {state.Lives}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            20,
            _textBrush
        );
        context.DrawText(livesText, new Point(size.Width - 120, 20));

        var waveText = new FormattedText(
            $"WAVE: {state.Wave}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            20,
            _textBrush
        );
        context.DrawText(waveText, new Point(size.Width / 2 - 50, 20));
    }

    private void RenderStartScreen(DrawingContext context, Size size)
    {
        var typeface = new Typeface("Courier New", FontStyle.Normal, FontWeight.Bold);
        var centerX = size.Width / 2;
        var centerY = size.Height / 2;

        // Game title
        var titleText = new FormattedText(
            "AVA RACE",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            48,
            _whiteTextBrush
        );
        context.DrawText(titleText, new Point(centerX - titleText.Width / 2, centerY - 280));

        // Subtitle
        var subtitleText = new FormattedText(
            "An Omega Race Clone",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            16,
            _whiteTextBrush
        );
        context.DrawText(subtitleText, new Point(centerX - subtitleText.Width / 2, centerY - 225));

        // Controls section (left side)
        var controlsHeader = new FormattedText(
            "─── CONTROLS ───",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            16,
            _whiteTextBrush
        );
        var controlsX = centerX - 200;
        context.DrawText(controlsHeader, new Point(controlsX - controlsHeader.Width / 2, centerY - 180));

        var controls = new[]
        {
            "← → or A D    Rotate",
            "↑ or W        Thrust",
            "SPACE         Fire",
            "P             Pause",
            "R             Restart",
            "CTRL+S        Screenshot",
            "ESC           Exit"
        };

        var yOffset = centerY - 150;
        foreach (var control in controls)
        {
            var controlText = new FormattedText(
                control,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12,
                _whiteTextBrush
            );
            context.DrawText(controlText, new Point(controlsX - controlText.Width / 2, yOffset));
            yOffset += 20;
        }

        // Enemies section (right side)
        var enemiesHeader = new FormattedText(
            "─── ENEMIES ───",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            16,
            _whiteTextBrush
        );
        var enemiesX = centerX + 200;
        context.DrawText(enemiesHeader, new Point(enemiesX - enemiesHeader.Width / 2, centerY - 180));

        // Draw enemy visuals
        DrawEnemyPreview(context, typeface, enemiesX, centerY - 130, "DROID", 100, DrawDroidShape);
        DrawEnemyPreview(context, typeface, enemiesX, centerY - 60, "FAST DROID", 200, DrawFastDroidShape);
        DrawEnemyPreview(context, typeface, enemiesX, centerY + 10, "HUNTER", 300, DrawHunterShape);

        // Start prompt (blinking effect using time)
        var showPrompt = (DateTime.Now.Millisecond / 500) % 2 == 0;
        if (showPrompt)
        {
            var startText = new FormattedText(
                "Press SPACE to Start",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                24,
                _whiteTextBrush
            );
            context.DrawText(startText, new Point(centerX - startText.Width / 2, centerY + 120));
        }
    }

    private void DrawEnemyPreview(DrawingContext context, Typeface typeface, double x, double y,
        string name, int points, Action<DrawingContext, double, double, double> drawShape)
    {
        // Draw the enemy shape
        drawShape(context, x - 80, y + 15, 1.5);

        // Draw name
        var nameText = new FormattedText(
            name,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            14,
            _whiteTextBrush
        );
        context.DrawText(nameText, new Point(x - 40, y));

        // Draw points
        var pointsText = new FormattedText(
            $"{points} pts",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            12,
            _textBrush
        );
        context.DrawText(pointsText, new Point(x - 40, y + 18));
    }

    private void DrawDroidShape(DrawingContext context, double x, double y, double scale)
    {
        var size = 12.0 * scale;
        var vertices = new Point[]
        {
            new(x + size, y),
            new(x, y + size),
            new(x - size, y),
            new(x, y - size)
        };
        DrawPolygon(context, vertices, _entityPen);
    }

    private void DrawFastDroidShape(DrawingContext context, double x, double y, double scale)
    {
        var size = 12.0 * 0.8 * scale;
        var vertices = new Point[]
        {
            new(x + size, y),
            new(x + size * 0.3, y + size * 0.5),
            new(x - size, y),
            new(x + size * 0.3, y - size * 0.5)
        };
        DrawPolygon(context, vertices, _entityPen);
    }

    private void DrawHunterShape(DrawingContext context, double x, double y, double scale)
    {
        var size = 12.0 * scale;
        var vertices = new Point[]
        {
            new(x + size, y),
            new(x + size * 0.5, y + size * 0.7),
            new(x - size * 0.5, y + size * 0.7),
            new(x - size, y),
            new(x - size * 0.5, y - size * 0.7),
            new(x + size * 0.5, y - size * 0.7)
        };
        DrawPolygon(context, vertices, _entityPen);
    }
}
