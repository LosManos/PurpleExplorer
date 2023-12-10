using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PurpleExplorer.Views;

public class MessageDetailsWindow : Window
{
    public MessageDetailsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void OnClose(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}