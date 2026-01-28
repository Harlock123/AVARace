using Avalonia.Controls;
using Avalonia.Input;
using AVARace.ViewModels;

namespace AVARace.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.InputHandler.HandleKeyDown(e.Key);

            if (e.Key == Key.Space && !vm.GameEngine.State.IsRunning && !vm.IsGameOver)
            {
                vm.StartGameCommand.Execute(null);
            }

            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        e.Handled = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.InputHandler.HandleKeyUp(e.Key);
        }

        e.Handled = true;
    }
}
