using Avalonia;

namespace AVARace.Game.Entities;

public class Explosion : Entity
{
    private readonly List<ExplosionParticle> _particles = new();
    private double _lifetime;
    private const double MaxLifetime = 0.8;

    public double Progress => _lifetime / MaxLifetime;
    public IReadOnlyList<ExplosionParticle> Particles => _particles;

    public Explosion(Vector2 position, int particleCount = 12, double speed = 150) : base(position)
    {
        CollisionRadius = 0; // Explosions don't collide
        _lifetime = 0;

        var random = new Random();
        for (int i = 0; i < particleCount; i++)
        {
            var angle = (2 * Math.PI * i / particleCount) + (random.NextDouble() - 0.5) * 0.5;
            var velocity = new Vector2(
                Math.Cos(angle) * speed * (0.5 + random.NextDouble()),
                Math.Sin(angle) * speed * (0.5 + random.NextDouble())
            );
            var length = 5 + random.NextDouble() * 10;
            _particles.Add(new ExplosionParticle(Position, velocity, angle, length));
        }
    }

    public override void Update(double deltaTime)
    {
        _lifetime += deltaTime;

        if (_lifetime >= MaxLifetime)
        {
            IsAlive = false;
            return;
        }

        foreach (var particle in _particles)
        {
            particle.Update(deltaTime);
        }
    }
}

public class ExplosionParticle
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }
    public double Angle { get; }
    public double Length { get; }

    public ExplosionParticle(Vector2 position, Vector2 velocity, double angle, double length)
    {
        Position = position;
        Velocity = velocity;
        Angle = angle;
        Length = length;
    }

    public void Update(double deltaTime)
    {
        Position += Velocity * deltaTime;
    }

    public Point StartPoint => new(Position.X, Position.Y);

    public Point EndPoint
    {
        get
        {
            var cos = Math.Cos(Angle);
            var sin = Math.Sin(Angle);
            return new Point(Position.X + cos * Length, Position.Y + sin * Length);
        }
    }
}
