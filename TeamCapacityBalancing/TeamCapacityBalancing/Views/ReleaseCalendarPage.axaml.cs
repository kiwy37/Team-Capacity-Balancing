using Avalonia.Controls;

namespace TeamCapacityBalancing.Views;

public partial class ReleaseCalendarPage : UserControl
{
    public ReleaseCalendarPage()
    {
        InitializeComponent();
        calendarGrid.SelectionChanged += (sender, e) =>
        {
            calendarGrid.SelectedItems.Clear();
        };
    }
}
