using Avalonia.Controls;
using System.IO;
using TeamCapacityBalancing.Models;
using TeamCapacityBalancing.Navigation;
using TeamCapacityBalancing.Services.LocalDataSerialization;
using TeamCapacityBalancing.ViewModels;

namespace TeamCapacityBalancing.Views
{
    public partial class DeleteLocalFilesWindow : Window
    {
        public User SelectedUser { get; set; }
        public ServiceCollection serviceCollection { get; set; }

        public DeleteLocalFilesWindow(string text)
        {
            InitializeComponent();
            message.Text = text;
        }

        public DeleteLocalFilesWindow()
        {
            InitializeComponent();
        }

        void Close(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        void Delete(object sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {

            File.Delete(JsonSerialization.UserFilePath + SelectedUser!.Username);
            File.Delete(JsonSerialization.UserStoryFilePath + SelectedUser.Username);
            File.Delete(JsonSerialization.SprintPath + SelectedUser.Username);
            File.Delete(JsonSerialization.SelectionShortTermPath + SelectedUser.Username);



            var mainWindow = serviceCollection.GetService(typeof(Window));
            var dialog = new SaveSuccessfulWindow("Local files have been deleted successfully");
            dialog.Title = "Delete Local Files";
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog((MainWindow)mainWindow);

            var vm = serviceCollection.GetService(typeof(BalancingViewModel));
            if (vm != null)
            {
                ((BalancingViewModel)vm).PopulateDefaultTeamUsers();
                ((BalancingViewModel)vm).RefreshClicked();
            }

            Close();

            
        }

    }
}
