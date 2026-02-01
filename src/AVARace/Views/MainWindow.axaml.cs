using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
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
        // Handle Ctrl+S for screenshot
        if (e.Key == Key.S && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            SaveScreenshot();
            e.Handled = true;
            return;
        }

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

    private void SaveScreenshot()
    {
        try
        {
            var canvas = this.FindControl<GameCanvas>("GameCanvas");
            if (canvas == null) return;

            var pixelSize = new Avalonia.PixelSize((int)canvas.Bounds.Width, (int)canvas.Bounds.Height);
            if (pixelSize.Width <= 0 || pixelSize.Height <= 0) return;

            using var bitmap = new RenderTargetBitmap(pixelSize);
            bitmap.Render(canvas);

            // Create screenshots directory if it doesn't exist
            var screenshotsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "AVARace"
            );
            Directory.CreateDirectory(screenshotsDir);

            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var filename = Path.Combine(screenshotsDir, $"AVARace_{timestamp}.png");

            using var stream = File.Create(filename);
            bitmap.Save(stream);

            Console.WriteLine($"Screenshot saved: {filename}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save screenshot: {ex.Message}");
        }
    }
}
