using AVARace.Game;
using AVARace.Game.Entities;

namespace AVARace.Services.Interfaces;

public interface IGameEngine
{
    GameState State { get; }
    IReadOnlyList<Entity> Entities { get; }
    Arena Arena { get; }

    void Start();
    void Stop();
    void Pause();
    void Resume();
    void Reset();
    void Update(double deltaTime);

    event Action? OnGameUpdated;
    event Action? OnGameOver;
}
