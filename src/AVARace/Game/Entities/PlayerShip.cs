using Avalonia;

namespace AVARace.Game.Entities;

public class PlayerShip : Entity
{
    private const double ThrustPower = 300.0;
    private const double MaxSpeed = 400.0;
    private const double RotationRate = 5.0;
    private const double Friction = 0.98;
    private const double ShipSize = 15.0;
    private const double FireCooldown = 0.2;
    private const double InvulnerabilityDuration = 2.0;

    private double _fireCooldownTimer;
    private double _invulnerabilityTimer;

    public bool CanFire => _fireCooldownTimer <= 0;
    public bool IsInvulnerable => _invulnerabilityTimer > 0;
    public double InvulnerabilityProgress => _invulnerabilityTimer / InvulnerabilityDuration;

    public PlayerShip(Vector2 position) : base(position)
    {
        CollisionRadius = ShipSize;
        Rotation = -Math.PI / 2;

        Vertices = new Point[]
        {
            new(ShipSize, 0),
            new(-ShipSize * 0.7, -ShipSize * 0.6),
            new(-ShipSize * 0.4, 0),
            new(-ShipSize * 0.7, ShipSize * 0.6)
        };
    }

    public void RotateLeft(double deltaTime)
    {
        Rotation -= RotationRate * deltaTime;
    }

    public void RotateRight(double deltaTime)
    {
        Rotation += RotationRate * deltaTime;
    }

    public void Thrust(double deltaTime)
    {
        var direction = Vector2.FromAngle(Rotation);
        Velocity += direction * ThrustPower * deltaTime;

        var speed = Velocity.Length;
        if (speed > MaxSpeed)
        {
            Velocity = Velocity.Normalized() * MaxSpeed;
        }
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);
        Velocity *= Friction;

        if (_fireCooldownTimer > 0)
        {
            _fireCooldownTimer -= deltaTime;
        }

        if (_invulnerabilityTimer > 0)
        {
            _invulnerabilityTimer -= deltaTime;
        }
    }

    public void MakeInvulnerable()
    {
        _invulnerabilityTimer = InvulnerabilityDuration;
    }

    public Bullet? Fire()
    {
        if (!CanFire) return null;

        _fireCooldownTimer = FireCooldown;
        var direction = Vector2.FromAngle(Rotation);
        var bulletPos = Position + direction * (CollisionRadius + 5);
        return new Bullet(bulletPos, direction);
    }

    public void ResetFireCooldown()
    {
        _fireCooldownTimer = 0;
    }
}
