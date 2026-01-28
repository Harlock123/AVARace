using Avalonia;

namespace AVARace.Game.Entities;

public class Bullet : Entity
{
    private const double BulletSpeed = 600.0;
    private const double BulletLifetime = 1.5;
    private const double BulletSize = 3.0;

    private double _lifetime;

    public Bullet(Vector2 position, Vector2 direction) : base(position)
    {
        Velocity = direction * BulletSpeed;
        CollisionRadius = BulletSize;
        _lifetime = BulletLifetime;

        Vertices = new Point[]
        {
            new(BulletSize, 0),
            new(-BulletSize, BulletSize * 0.5),
            new(-BulletSize, -BulletSize * 0.5)
        };

        Rotation = Math.Atan2(direction.Y, direction.X);
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);
        _lifetime -= deltaTime;

        if (_lifetime <= 0)
        {
            IsAlive = false;
        }
    }
}
