using Avalonia.Controls;

namespace TeamCapacityBalancing.Views
{
    public partial class SprintSelectionPage : UserControl
    {
        public SprintSelectionPage()
        {
            InitializeComponent();
            sprintGrid.SelectionChanged += (sender, e) =>
            {
                sprintGrid.SelectedItems.Clear();
            };
        }
    }
}
