using Avalonia;

namespace AVARace.Game.Entities;

public enum EnemyType
{
    Droid,
    FastDroid,
    Hunter
}

public class Enemy : Entity
{
    private const double DroidSize = 12.0;
    private const double BaseSpeed = 80.0;

    public EnemyType Type { get; }
    public int PointValue { get; }

    private readonly Random _random = new();
    private double _directionChangeTimer;
    private double _directionChangeInterval;
    private Vector2? _targetPosition;

    public Enemy(Vector2 position, EnemyType type = EnemyType.Droid) : base(position)
    {
        Type = type;
        CollisionRadius = DroidSize;

        switch (type)
        {
            case EnemyType.Droid:
                PointValue = 100;
                SetupDroidShape();
                SetRandomVelocity(BaseSpeed);
                _directionChangeInterval = 2.0 + _random.NextDouble() * 2.0;
                break;

            case EnemyType.FastDroid:
                PointValue = 200;
                SetupFastDroidShape();
                SetRandomVelocity(BaseSpeed * 1.5);
                _directionChangeInterval = 1.0 + _random.NextDouble() * 1.5;
                break;

            case EnemyType.Hunter:
                PointValue = 300;
                SetupHunterShape();
                _directionChangeInterval = 0.5;
                break;
        }
    }

    private void SetupDroidShape()
    {
        Vertices = new Point[]
        {
            new(DroidSize, 0),
            new(0, DroidSize),
            new(-DroidSize, 0),
            new(0, -DroidSize)
        };
    }

    private void SetupFastDroidShape()
    {
        var size = DroidSize * 0.8;
        Vertices = new Point[]
        {
            new(size, 0),
            new(size * 0.3, size * 0.5),
            new(-size, 0),
            new(size * 0.3, -size * 0.5)
        };
    }

    private void SetupHunterShape()
    {
        Vertices = new Point[]
        {
            new(DroidSize, 0),
            new(DroidSize * 0.5, DroidSize * 0.7),
            new(-DroidSize * 0.5, DroidSize * 0.7),
            new(-DroidSize, 0),
            new(-DroidSize * 0.5, -DroidSize * 0.7),
            new(DroidSize * 0.5, -DroidSize * 0.7)
        };
    }

    private void SetRandomVelocity(double speed)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        Velocity = Vector2.FromAngle(angle) * speed;
    }

    public void SetTarget(Vector2 position)
    {
        _targetPosition = position;
    }

    public override void Update(double deltaTime)
    {
        _directionChangeTimer += deltaTime;

        if (Type == EnemyType.Hunter && _targetPosition.HasValue)
        {
            var direction = (_targetPosition.Value - Position).Normalized();
            Velocity = direction * BaseSpeed * 1.2;
            Rotation = Math.Atan2(direction.Y, direction.X);
        }
        else if (_directionChangeTimer >= _directionChangeInterval)
        {
            _directionChangeTimer = 0;
            var currentSpeed = Velocity.Length;
            if (currentSpeed > 0)
            {
                var angleChange = (_random.NextDouble() - 0.5) * Math.PI * 0.5;
                var currentAngle = Math.Atan2(Velocity.Y, Velocity.X);
                var newAngle = currentAngle + angleChange;
                Velocity = Vector2.FromAngle(newAngle) * currentSpeed;
            }
        }

        Rotation = Math.Atan2(Velocity.Y, Velocity.X);
        base.Update(deltaTime);
    }
}
