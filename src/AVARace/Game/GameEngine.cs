using AVARace.Game.Entities;
using AVARace.Game.Physics;
using AVARace.Services.Interfaces;

namespace AVARace.Game;

public class GameEngine : IGameEngine
{
    private readonly IInputHandler _inputHandler;
    private readonly ISoundService _soundService;
    private readonly List<Entity> _entities = new();
    private readonly List<Bullet> _bullets = new();
    private readonly List<Enemy> _enemies = new();
    private readonly List<Explosion> _explosions = new();
    private readonly Random _random = new();

    private PlayerShip? _player;
    private Arena _arena;
    private double _respawnTimer;
    private const double RespawnDelay = 2.0;
    private bool _isRespawning;
    private bool _wasThrusting;

    public GameState State { get; } = new();
    public IReadOnlyList<Entity> Entities => _entities.AsReadOnly();
    public Arena Arena => _arena;

    public event Action? OnGameUpdated;
    public event Action? OnGameOver;

    public GameEngine(IInputHandler inputHandler, ISoundService soundService)
    {
        _inputHandler = inputHandler;
        _soundService = soundService;
        _arena = new Arena(800, 600);
        _soundService.Initialize();
    }

    public void SetArenaSize(double width, double height)
    {
        _arena = new Arena(width, height);
    }

    public void Start()
    {
        if (State.IsRunning) return;

        State.IsRunning = true;
        State.IsPaused = false;
        SpawnPlayer();
        SpawnWave();
    }

    public void Stop()
    {
        State.IsRunning = false;
    }

    public void Pause()
    {
        if (!State.IsRunning || State.IsGameOver) return;
        State.IsPaused = true;
    }

    public void Resume()
    {
        if (!State.IsRunning || State.IsGameOver) return;
        State.IsPaused = false;
    }

    public void Reset()
    {
        _entities.Clear();
        _bullets.Clear();
        _enemies.Clear();
        _explosions.Clear();
        _player = null;
        _isRespawning = false;
        _respawnTimer = 0;
        State.Reset();
        _inputHandler.Reset();
    }

    public void Update(double deltaTime)
    {
        if (!State.IsRunning || State.IsPaused || State.IsGameOver) return;

        HandleRespawn(deltaTime);
        HandleInput(deltaTime);
        UpdateEntities(deltaTime);
        HandleCollisions();
        CleanupDeadEntities();
        CheckWaveComplete();

        OnGameUpdated?.Invoke();
    }

    private void HandleRespawn(double deltaTime)
    {
        if (!_isRespawning) return;

        _respawnTimer -= deltaTime;
        if (_respawnTimer <= 0)
        {
            _isRespawning = false;
            SpawnPlayer();
        }
    }

    private void HandleInput(double deltaTime)
    {
        if (_player == null || !_player.IsAlive) return;

        if (_inputHandler.IsRotatingLeft)
        {
            _player.RotateLeft(deltaTime);
        }

        if (_inputHandler.IsRotatingRight)
        {
            _player.RotateRight(deltaTime);
        }

        var isThrusting = _inputHandler.IsThrusting;
        if (isThrusting)
        {
            _player.Thrust(deltaTime);
        }

        if (isThrusting != _wasThrusting)
        {
            _soundService.PlayThruster(isThrusting);
            _wasThrusting = isThrusting;
        }

        if (_inputHandler.IsFiring)
        {
            var bullet = _player.Fire();
            if (bullet != null)
            {
                _bullets.Add(bullet);
                _entities.Add(bullet);
                _soundService.PlayShoot();
            }
        }
    }

    private void UpdateEntities(double deltaTime)
    {
        foreach (var entity in _entities)
        {
            entity.Update(deltaTime);
            CollisionDetection.HandleWallCollision(entity, _arena);
        }

        foreach (var enemy in _enemies.Where(e => e.Type == EnemyType.Hunter && e.IsAlive))
        {
            if (_player != null && _player.IsAlive)
            {
                enemy.SetTarget(_player.Position);
            }
        }
    }

    private void HandleCollisions()
    {
        foreach (var bullet in _bullets.Where(b => b.IsAlive))
        {
            foreach (var enemy in _enemies.Where(e => e.IsAlive))
            {
                if (CollisionDetection.CheckCollision(bullet, enemy))
                {
                    bullet.IsAlive = false;
                    enemy.IsAlive = false;
                    State.AddScore(enemy.PointValue);
                    _soundService.PlayExplosion();
                    SpawnExplosion(enemy.Position);
                }
            }
        }

        if (_player != null && _player.IsAlive && !_player.IsInvulnerable)
        {
            foreach (var enemy in _enemies.Where(e => e.IsAlive))
            {
                if (CollisionDetection.CheckCollision(_player, enemy))
                {
                    var playerPos = _player.Position;
                    _player.IsAlive = false;
                    enemy.IsAlive = false;
                    _soundService.PlayExplosion();
                    _soundService.PlayThruster(false);
                    _wasThrusting = false;
                    SpawnExplosion(playerPos, 16, 200); // Bigger explosion for player
                    SpawnExplosion(enemy.Position);
                    PlayerDied();
                    break;
                }
            }
        }
    }

    private void SpawnExplosion(Vector2 position, int particleCount = 12, double speed = 150)
    {
        var explosion = new Explosion(position, particleCount, speed);
        _explosions.Add(explosion);
        _entities.Add(explosion);
    }

    private void PlayerDied()
    {
        State.LoseLife();

        if (State.IsGameOver)
        {
            OnGameOver?.Invoke();
        }
        else
        {
            _isRespawning = true;
            _respawnTimer = RespawnDelay;
        }
    }

    private void CleanupDeadEntities()
    {
        _entities.RemoveAll(e => !e.IsAlive);
        _bullets.RemoveAll(b => !b.IsAlive);
        _enemies.RemoveAll(e => !e.IsAlive);
        _explosions.RemoveAll(e => !e.IsAlive);
    }

    private void CheckWaveComplete()
    {
        if (_enemies.Count == 0 && !_isRespawning)
        {
            State.NextWave();
            SpawnWave();
        }
    }

    private void SpawnPlayer()
    {
        var startPos = _arena.GetPlayerStartPosition();
        _player = new PlayerShip(startPos);
        _player.MakeInvulnerable();
        _entities.Add(_player);
    }

    private void SpawnWave()
    {
        var enemyCount = 2 + State.Wave;
        var hasHunter = State.Wave >= 3;
        var hasFastDroids = State.Wave >= 2;

        // Get player position to avoid spawning enemies too close
        var playerPos = _player?.Position;

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyType type;
            if (hasHunter && i == 0)
            {
                type = EnemyType.Hunter;
            }
            else if (hasFastDroids && i < enemyCount / 2)
            {
                type = EnemyType.FastDroid;
            }
            else
            {
                type = EnemyType.Droid;
            }

            var pos = _arena.GetRandomSpawnPosition(_random, playerPos, 150);
            var enemy = new Enemy(pos, type);
            _enemies.Add(enemy);
            _entities.Add(enemy);
        }
    }
}
