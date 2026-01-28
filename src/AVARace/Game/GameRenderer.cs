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

    public GameRenderer(IGameEngine gameEngine)
    {
        _gameEngine = gameEngine;

        _arenaPen = new Pen(new SolidColorBrush(DimGreen), 2);
        _entityPen = new Pen(new SolidColorBrush(VectorGreen), 2);
        _bulletPen = new Pen(new SolidColorBrush(BrightGreen), 2);
        _playerPen = new Pen(new SolidColorBrush(VectorGreen), 2.5);
        _textBrush = new SolidColorBrush(VectorGreen);
    }

    public void Render(DrawingContext context, Size size)
    {
        context.FillRectangle(Brushes.Black, new Rect(size));

        if (_gameEngine is GameEngine engine)
        {
            engine.SetArenaSize(size.Width, size.Height);
        }

        RenderArena(context);
        RenderEntities(context);
        RenderHUD(context, size);
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
}
