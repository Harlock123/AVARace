using Avalonia;

namespace AVARace.Game.Entities;

public class Arena
{
    public double Width { get; }
    public double Height { get; }
    public double BorderThickness { get; } = 40;

    public Point[] OuterBoundary { get; }
    public Point[] InnerBoundary { get; }
    public Point[] CentralObstacle { get; }

    public double CenterX => Width / 2;
    public double CenterY => Height / 2;

    public Arena(double width, double height)
    {
        Width = width;
        Height = height;

        OuterBoundary = CreateOctagon(CenterX, CenterY, Math.Min(Width, Height) / 2 - 20);
        InnerBoundary = CreateOctagon(CenterX, CenterY, Math.Min(Width, Height) / 2 - BorderThickness - 20);
        CentralObstacle = CreateOctagon(CenterX, CenterY, Math.Min(Width, Height) / 8);
    }

    private static Point[] CreateOctagon(double cx, double cy, double radius)
    {
        var points = new Point[8];
        for (int i = 0; i < 8; i++)
        {
            var angle = i * Math.PI / 4 - Math.PI / 8;
            points[i] = new Point(
                cx + radius * Math.Cos(angle),
                cy + radius * Math.Sin(angle)
            );
        }
        return points;
    }

    public (bool collided, Vector2 normal) CheckWallCollision(Vector2 position, double radius)
    {
        var toCenterFromPos = new Vector2(CenterX - position.X, CenterY - position.Y);
        var distanceFromCenter = toCenterFromPos.Length;

        var outerRadius = Math.Min(Width, Height) / 2 - BorderThickness - 20 - radius;
        if (distanceFromCenter > outerRadius)
        {
            var normal = toCenterFromPos.Normalized();
            return (true, normal);
        }

        var innerRadius = Math.Min(Width, Height) / 8 + radius;
        if (distanceFromCenter < innerRadius)
        {
            var normal = (-toCenterFromPos).Normalized();
            return (true, normal);
        }

        return (false, Vector2.Zero);
    }

    public Vector2 ReflectVelocity(Vector2 velocity, Vector2 normal)
    {
        var dot = Vector2.Dot(velocity, normal);
        return velocity - 2 * dot * normal;
    }

    public Vector2 GetRandomSpawnPosition(Random random)
    {
        var minRadius = Math.Min(Width, Height) / 8 + 50;
        var maxRadius = Math.Min(Width, Height) / 2 - BorderThickness - 70;
        var radius = minRadius + random.NextDouble() * (maxRadius - minRadius);
        var angle = random.NextDouble() * Math.PI * 2;

        return new Vector2(
            CenterX + radius * Math.Cos(angle),
            CenterY + radius * Math.Sin(angle)
        );
    }

    public Vector2 GetPlayerStartPosition()
    {
        var radius = (Math.Min(Width, Height) / 8 + Math.Min(Width, Height) / 2 - BorderThickness - 20) / 2;
        return new Vector2(CenterX + radius, CenterY);
    }
}
