using Avalonia;

namespace AVARace.Game.Entities;

public abstract class Entity
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public double Rotation { get; set; }
    public double RotationSpeed { get; set; }
    public bool IsAlive { get; set; } = true;
    public double CollisionRadius { get; set; }

    public Point[] Vertices { get; protected set; } = Array.Empty<Point>();

    protected Entity(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Rotation = 0;
    }

    public virtual void Update(double deltaTime)
    {
        Position += Velocity * deltaTime;
        Rotation += RotationSpeed * deltaTime;
    }

    public Point[] GetTransformedVertices()
    {
        var cos = Math.Cos(Rotation);
        var sin = Math.Sin(Rotation);
        var transformed = new Point[Vertices.Length];

        for (int i = 0; i < Vertices.Length; i++)
        {
            var x = Vertices[i].X * cos - Vertices[i].Y * sin + Position.X;
            var y = Vertices[i].X * sin + Vertices[i].Y * cos + Position.Y;
            transformed[i] = new Point(x, y);
        }

        return transformed;
    }

    public bool CollidesWith(Entity other)
    {
        var distance = (Position - other.Position).Length;
        return distance < (CollisionRadius + other.CollisionRadius);
    }
}

public struct Vector2
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vector2(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double Length => Math.Sqrt(X * X + Y * Y);

    public Vector2 Normalized()
    {
        var len = Length;
        if (len == 0) return Zero;
        return new Vector2(X / len, Y / len);
    }

    public static Vector2 Zero => new(0, 0);

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 v, double scalar) => new(v.X * scalar, v.Y * scalar);
    public static Vector2 operator *(double scalar, Vector2 v) => new(v.X * scalar, v.Y * scalar);
    public static Vector2 operator /(Vector2 v, double scalar) => new(v.X / scalar, v.Y / scalar);
    public static Vector2 operator -(Vector2 v) => new(-v.X, -v.Y);

    public static double Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

    public static Vector2 FromAngle(double angle) => new(Math.Cos(angle), Math.Sin(angle));
}
