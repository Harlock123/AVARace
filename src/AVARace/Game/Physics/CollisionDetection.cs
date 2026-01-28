using AVARace.Game.Entities;

namespace AVARace.Game.Physics;

public static class CollisionDetection
{
    public static bool CheckCollision(Entity a, Entity b)
    {
        if (!a.IsAlive || !b.IsAlive) return false;
        return a.CollidesWith(b);
    }

    public static void HandleWallCollision(Entity entity, Arena arena)
    {
        var (collided, normal) = arena.CheckWallCollision(entity.Position, entity.CollisionRadius);

        if (collided)
        {
            entity.Velocity = arena.ReflectVelocity(entity.Velocity, normal);
            entity.Position += normal * 2;
        }
    }

    public static List<(Entity a, Entity b)> FindCollisions(IEnumerable<Entity> entitiesA, IEnumerable<Entity> entitiesB)
    {
        var collisions = new List<(Entity, Entity)>();

        foreach (var a in entitiesA)
        {
            if (!a.IsAlive) continue;

            foreach (var b in entitiesB)
            {
                if (!b.IsAlive) continue;
                if (ReferenceEquals(a, b)) continue;

                if (CheckCollision(a, b))
                {
                    collisions.Add((a, b));
                }
            }
        }

        return collisions;
    }
}
