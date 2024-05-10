using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TeamCapacityBalancing.Models;
using TeamCapacityBalancing.Navigation;
using TeamCapacityBalancing.Services.LocalDataSerialization;
using TeamCapacityBalancing.Services.Postgres_connection;
using TeamCapacityBalancing.Services.ServicesAbstractions;
using TeamCapacityBalancing.Views;

namespace TeamCapacityBalancing.ViewModels;

public sealed partial class TeamViewModel : ObservableObject
{
    //NavigationServices
    private ServiceCollection? _serviceCollection;
    private readonly PageService? _pageService;
    private readonly NavigationService? _navigationService;
    

    //DataServices
    private readonly IDataSerialization _jsonSerialization = new JsonSerialization();
    private readonly IDataProvider _queriesForDataBase = new QueriesForDataBase();

    //Properties
    public List<PageData>? Pages { get; }
    public string TeamLeaderUsername { get; set; }

    [ObservableProperty]
    private bool _addAllFromTeamEnable;

    [ObservableProperty]
    private bool _resourcePageVisibility;

    [ObservableProperty]
    private bool _removeAllFromTeamEnable;

    [ObservableProperty]
    private bool _addToTeamEnabledVisibility;

    [ObservableProperty]
    private bool _removeFromTeamVisibility;

    [ObservableProperty]
    private ObservableCollection<User> _allUsers = new();

    [ObservableProperty]
    private ObservableCollection<User>? _yourTeam;

    private User? _selectedUserYourTeam;
    public User? SelectedUserYourTeam
    {
        get { return _selectedUserYourTeam; }
        set
        {
            if (value != null)
            {
                SelectedUserAllUsers = null;
                ResourcePageVisibility = true;
                RemoveFromTeamVisibility = true;
            }
            else
            {
                ResourcePageVisibility = false;
                RemoveFromTeamVisibility = false;
            }
            AddToTeamEnabledVisibility = false;
            _selectedUserYourTeam = value;
            OnPropertyChanged(nameof(SelectedUserYourTeam));
        }
    }

    private User? _selectedUserAllUsers;
    public User? SelectedUserAllUsers
    {
        get { return _selectedUserAllUsers; }
        set
        {
            if (value != null)
            {
                SelectedUserYourTeam = null;
                AddToTeamEnabledVisibility = true;
            }
            else
            {
                AddToTeamEnabledVisibility = false;
            }
            ResourcePageVisibility = false;
            RemoveFromTeamVisibility = false;
            _selectedUserAllUsers = value;
            OnPropertyChanged(nameof(SelectedUserAllUsers));
        }
    }



    //Constructors
    public TeamViewModel()
    {
        TeamLeaderUsername = string.Empty;
    }

    public TeamViewModel(ServiceCollection serviceCollection, PageService pageService, NavigationService navigationService)
    {
        _serviceCollection = serviceCollection;
        _pageService = pageService;
        _navigationService = navigationService;
        Pages = _pageService.Pages.Select(x => x.Value).Where(x => x.ViewModelType != this.GetType()).ToList();
        TeamLeaderUsername = string.Empty;
    }

    

    
    //Private methods
    private void CheckButtonsVisibility()
    {
        if (YourTeam!=null && YourTeam.Count == 0)
        {
            RemoveAllFromTeamEnable = false;
            RemoveFromTeamVisibility = false;
        }
        else
        {
            RemoveAllFromTeamEnable = true;
        }
        if (AllUsers.Count() == 0)
        {
            AddAllFromTeamEnable = false;
            AddToTeamEnabledVisibility = false;
        }
        else
        {
            AddAllFromTeamEnable = true;
        }
    }

    public void PopulateUsersLists(string teamLeaderUsername)
    {
        TeamLeaderUsername = teamLeaderUsername;
        YourTeam = new ObservableCollection<User>(_jsonSerialization.DeserializeTeamData(TeamLeaderUsername));
        AllUsers = new ObservableCollection<User>(_queriesForDataBase.GetAllUsers());
        foreach (var user in YourTeam)
        {
            var u = AllUsers.FirstOrDefault(u => u.Id == user.Id);
            if (u != null)
            {
                AllUsers.Remove(u);
            }
        }
        CheckButtonsVisibility();
    }

    //Commands

    [RelayCommand]
    public void AddAllToTeam()
    {
        YourTeam = new ObservableCollection<User>(AllUsers.Concat(YourTeam));
        AllUsers = new();
        foreach (var user in YourTeam)
        {
            user.HasTeam = true;
        }
        CheckButtonsVisibility();
        _jsonSerialization.SerializeTeamData(YourTeam.ToList(), TeamLeaderUsername);
    }

    [RelayCommand]
    public void RemoveAllFromTeam()
    {
        AllUsers = new ObservableCollection<User>(_queriesForDataBase.GetAllUsers());
        YourTeam = new();
        CheckButtonsVisibility();
        _jsonSerialization.SerializeTeamData(YourTeam.ToList(), TeamLeaderUsername);
    }

    [RelayCommand]
    public void RemoveFromTeam()
    {
        if (SelectedUserYourTeam != null && YourTeam!=null)
        {
            var existingUser = YourTeam.FirstOrDefault(u => u.Id == SelectedUserYourTeam.Id);


            if (existingUser != null)
            {
                AllUsers.Add(existingUser);
                existingUser.HasTeam = false;
                YourTeam.Remove(existingUser);
            }
        }
        CheckButtonsVisibility();
        _jsonSerialization.SerializeTeamData(YourTeam.ToList(), TeamLeaderUsername);
    }

    [RelayCommand]
    public void AddToTeam()
    {
        if (SelectedUserAllUsers != null)
        {
            var existingUser = AllUsers.FirstOrDefault(u => u.Id == SelectedUserAllUsers.Id);
            if (existingUser != null)
            {
                if (YourTeam != null)
                {
                    YourTeam.Add(existingUser);
                }
                existingUser.HasTeam = true;
                AllUsers.Remove(existingUser);
            }
        }

        CheckButtonsVisibility();
        _jsonSerialization.SerializeTeamData(YourTeam.ToList(), TeamLeaderUsername);
    }

    [RelayCommand]
    public void ResourcePage()
    {
        if (_serviceCollection != null)
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.DataContext = new InputViewModel(_serviceCollection);

            if (SelectedUserYourTeam != null)
            {
                var mainWindow = _serviceCollection.GetService(typeof(Window));
                if (mainWindow != null)
                {
                    inputWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    inputWindow.ShowDialog((MainWindow)mainWindow);
                }
            }
        }
        SelectedUserAllUsers = null;
    }

    [RelayCommand]
    public void BackToPage()
    {
        if (_serviceCollection != null)
        {
            var vm = _serviceCollection.GetService(typeof(BalancingViewModel));
            if (vm != null)
            {
                ((BalancingViewModel)vm).SyncTeamWithBalancingPageData();
                ((BalancingViewModel)vm).GetOpenTasks();
               ((BalancingViewModel)vm).IsBalancing = false;
            }
        }

        if(_navigationService != null)
        {
            _navigationService.CurrentPageType = typeof(BalancingPage);
        }
        

        //var window = _serviceCollection.GetService(typeof(Avalonia.Controls.Window));

        //if (window != null)
        //{
        //    ((MainWindow)window).WindowState = Avalonia.Controls.WindowState.Normal;
        //    ((MainWindow)window).WindowState = Avalonia.Controls.WindowState.Maximized;

        //}

        SelectedUserYourTeam = null;
        SelectedUserAllUsers = null;

        _jsonSerialization.SerializeTeamData(YourTeam.ToList(), TeamLeaderUsername);
    }


}
