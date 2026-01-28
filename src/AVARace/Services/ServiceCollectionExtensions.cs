using AVARace.Game;
using AVARace.Game.Input;
using AVARace.Services.Interfaces;
using AVARace.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AVARace.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAVARaceServices(this IServiceCollection services)
    {
        services.AddSingleton<IControllerService, ControllerService>();
        services.AddSingleton<IInputHandler, InputHandler>();
        services.AddSingleton<ISoundService, SoundService>();
        services.AddSingleton<IGameEngine, GameEngine>();
        services.AddSingleton<GameRenderer>();
        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}
