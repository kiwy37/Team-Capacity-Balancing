using Avalonia.Controls;

namespace TeamCapacityBalancing.Views;

public partial class InputWindow : Window
{
    public InputWindow()
    {
        InitializeComponent();
    }

    void Close(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
