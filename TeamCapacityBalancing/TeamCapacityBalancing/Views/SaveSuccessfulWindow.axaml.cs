using Avalonia.Controls;

namespace TeamCapacityBalancing.Views;

public partial class SaveSuccessfulWindow : Window
{
    public SaveSuccessfulWindow(string text)
    {
        InitializeComponent();
        message.Text = text;
    }
    
    public SaveSuccessfulWindow()
    {
        InitializeComponent();
    }

    void Close(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}