using Avalonia.Controls;

namespace MTGcollector_app.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }
}