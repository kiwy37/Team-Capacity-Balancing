using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TeamCapacityBalancing.Models;
using TeamCapacityBalancing.Navigation;
using TeamCapacityBalancing.Services.LocalDataSerialization;
using TeamCapacityBalancing.Services.Postgres_connection;
using TeamCapacityBalancing.Services.ServicesAbstractions;
using TeamCapacityBalancing.Views;

namespace TeamCapacityBalancing.ViewModels;

public sealed partial class BalancingViewModel : ObservableObject
{
    //private variables
    private readonly PageService? _pageService;
    private readonly NavigationService? _navigationService;
    private readonly ServiceCollection _serviceCollection;
    private const int MaxNumberOfUsers = 10;
    private List<IssueData> allStories = new();
    private List<UserStoryAssociation> allUserStoryAssociation = new();
    private int currentEpicId = 0;
    private List<Tuple<User, float>> totalWork;
    private HashSet<string> businessCaseSet = new();
    public DateTime finishDate;

    //services
    private readonly IDataProvider _queriesForDataBase = new QueriesForDataBase();
    private readonly IDataSerialization _jsonSerialization = new JsonSerialization();


    //Observable properties
    [ObservableProperty]
    public List<User> _allUsers;

    [ObservableProperty]
    public List<OpenTasksUserAssociation> _openTasks;

    [ObservableProperty]
    private bool _isShortTermVisible = false;

    [ObservableProperty]
    private bool _isBalancing = false;

    [ObservableProperty]
    private bool _isPaneOpen = true;

    [ObservableProperty]
    private bool _getStories = false;

    [ObservableProperty]
    private bool _miniMessage = true;

    [ObservableProperty]
    private SplitViewDisplayMode _mode = SplitViewDisplayMode.CompactInline;

    [ObservableProperty]
    public ObservableCollection<UserStoryAssociation> _myUserAssociation = new ();

    [ObservableProperty]
    public ObservableCollection<UserStoryAssociation> _shortTermStoryes;

    [ObservableProperty]
    private ObservableCollection<User> _teamMembers;


    //Properties
    public ObservableCollection<IssueData> Epics { get; set; } = new();

    public ObservableCollection<string> BusinessCase { get; set; } = new();

    public List<float> remaining = new();

    public ObservableCollection<UserStoryAssociation> Balancing { get; set; } = new ObservableCollection<UserStoryAssociation>
    {
           new UserStoryAssociation(
                new IssueData("Balancing", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                true,
                3.0f,
                 new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                MaxNumberOfUsers
            ),
    };

    private string _filterString;
    public string FilterString
    {
        get
        {
            return _filterString;
        }

        set
        {
            _filterString = value;
            Filter();
            OnPropertyChanged(nameof(SelectedUser));
        }
    }

    private User? _selectedUser;
    public User? SelectedUser
    {
        get { return _selectedUser; }
        set
        {
            if (_selectedUser != value)
            {
                MiniMessage = false;
                _selectedUser = value;

                if (value != null)
                {
                    OnSelectedUser();
                }

                //OrderTeamAndStoryInfo();

                OnPropertyChanged(nameof(SelectedUser));
            }
        }
    }

    public ObservableCollection<UserStoryAssociation> Totals { get; set; } = new ObservableCollection<UserStoryAssociation>()
    {
        new UserStoryAssociation(
                 new IssueData("Open tasks", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                 MaxNumberOfUsers
             ),

        new UserStoryAssociation(
                 new IssueData("Total work open story", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                 MaxNumberOfUsers
             ),
          new UserStoryAssociation(
                 new IssueData("Total work", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                 MaxNumberOfUsers
             ),
          new UserStoryAssociation(
                 new IssueData("Total capacity", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                 MaxNumberOfUsers
             ),

    };

    //Constructors
    public BalancingViewModel()
    {

    }

    public BalancingViewModel(PageService pageService, NavigationService navigationService, ServiceCollection serviceCollection)
    {
        _pageService = pageService;
        _navigationService = navigationService;
        _serviceCollection = serviceCollection;
        PopulateDefaultTeamUsers();
        ShowShortTermStoryes();
        AllUsers = _queriesForDataBase.GetAllTeamLeaders();
        //OpenTasks = _queriesForDataBase.GetRemainingForUser();

    }



    //Private methods
    private void OnSelectedUser()
    {
        currentEpicId = -1;
        GetStories = false;
        IsBalancing = false;
        IsShortTermVisible = false;
        MyUserAssociation.Clear();
        allUserStoryAssociation.Clear();
        BusinessCase.Clear();
        FilterString = "None";
        businessCaseSet.Clear();
        List<IssueData> epics;
        epics = _queriesForDataBase.GetAllEpicsByTeamLeader(SelectedUser);
        allStories = _queriesForDataBase.GetAllStoriesByTeamLeader(SelectedUser);
        if (epics != null)
        {
            Epics = new ObservableCollection<IssueData>(epics);
            OnPropertyChanged("Epics");
        }
        if (File.Exists(JsonSerialization.UserFilePath + SelectedUser.Username))
        {
            GetTeamUsers();
        }
        GetOpenTasks();
        GetBusinessCaseForEpics();
        ShowAllStories();
    }

    private void GetTeamUsers()
    {
        if(SelectedUser == null) return;
        var aux = new ObservableCollection<User>(_jsonSerialization.DeserializeTeamData(SelectedUser.Username));
        for (int i = aux.Count; i < MaxNumberOfUsers; i++)
        {
            User newUser = new User("default");
            newUser.HasTeam = false;
            aux.Add(newUser);
        }
        for (int i = 0; i < MaxNumberOfUsers; i++)
        {
            TeamMembers[i] = aux[i];
        }
    }

    public void PopulateDefaultTeamUsers()
    {
        List<User> aux = new();
        for (int i = 0; i < MaxNumberOfUsers; i++)
        {
            User newUser = new User("default}");
            newUser.HasTeam = false;
            aux.Add(newUser);
        }
        TeamMembers = new ObservableCollection<User>(aux);
    }

    private void SerializeStoryData()
    {
        List<UserStoryDataSerialization> userStoryDataSerializations = new();

        for (int j = 0; j < allUserStoryAssociation.Count; j++)
        {
            List<Tuple<User, float>> capacityList = new List<Tuple<User, float>>();
            for (int i = 0; i < MaxNumberOfUsers; i++)
            {
                capacityList.Add(new Tuple<User, float>(TeamMembers[i], allUserStoryAssociation[j].Days[i].Value));
            }
            userStoryDataSerializations.Add(new UserStoryDataSerialization(allUserStoryAssociation[j].StoryData, allUserStoryAssociation[j].ShortTerm, allUserStoryAssociation[j].Remaining, capacityList));
        }
        if(SelectedUser!= null) 
        _jsonSerialization.SerializeUserStoryData(userStoryDataSerializations, SelectedUser.Username);
    }

    private void GetSerializedData()
    {

        List<UserStoryDataSerialization> userStoryDataSerializations = new();
        userStoryDataSerializations = _jsonSerialization.DeserializeUserStoryData(SelectedUser.Username);
        foreach (UserStoryDataSerialization ser in userStoryDataSerializations)
        {
            List<Tuple<User, float>> capacityList = new List<Tuple<User, float>>();
            for (int i = 0; i < ser.UsersCapacity.Count; i++)
            {
                capacityList.Add(new(ser.UsersCapacity[i].Item1, ser.UsersCapacity[i].Item2));
            }

            allUserStoryAssociation.Add(new UserStoryAssociation(ser.Story, ser.ShortTerm, ser.Remaining, capacityList, MaxNumberOfUsers));
            MyUserAssociation.Add(allUserStoryAssociation.Last());
        }
    }

    private void PopulateByDefault()
    {
        List<Tuple<User, float>> capacityList = GenerateDefaultDays();

        foreach (IssueData story in allStories)
        {
            allUserStoryAssociation.Add(new UserStoryAssociation(story, false, story.Remaining, capacityList, MaxNumberOfUsers));
            MyUserAssociation.Add(allUserStoryAssociation.Last());
        }

    }

    private void OrderTeamAndStoryInfo()
    {
        TeamMembers = new ObservableCollection<User>(TeamMembers.OrderBy(m => m.Username));
        foreach (UserStoryAssociation userStoryAssociation in MyUserAssociation)
        {
            userStoryAssociation.Days = new ObservableCollection<Wrapper<float>>(userStoryAssociation.Days.OrderBy(m => m.UserName));
        }
        OpenTasks = OpenTasks.OrderBy(x => x.User.Username).ToList();

    }

    public void GetOpenTasks()
    {
        OpenTasks = _queriesForDataBase.GetRemainingForUser();

        List<OpenTasksUserAssociation> aux = new();
        foreach (var user in TeamMembers)
        {
            OpenTasksUserAssociation? oT = OpenTasks.FirstOrDefault(x => x.User.Id == user.Id);
            if (oT != null)
            {
                aux.Add(new OpenTasksUserAssociation(user, oT.Remaining));
            }
            else
            {
                aux.Add(new OpenTasksUserAssociation(user, 0));
            }
        }
        while (aux.Count < MaxNumberOfUsers)
        {
            aux.Add(new OpenTasksUserAssociation(new User
            {
                Username = "default"
            }, 0));
        }
        OpenTasks = aux;
    }

    public void CreateDefaultListWithDays
        (List<Wrapper<float>> defaultList)
    {
        defaultList.Clear();
        for (int i = 0; i < MaxNumberOfUsers; i++)
        {
            defaultList.Add(new Wrapper<float> { UserName = "default", Value = 0 });
        }
    }

    public void SyncTeamWithBalancingPageData()
    {
        if(SelectedUser == null) return;
        if (File.Exists(JsonSerialization.UserFilePath + SelectedUser.Username))
        {
            GetTeamUsers();
        }

        List<User> teamMembers = TeamMembers.Where(user => user.Username != "default").ToList();
        List<Wrapper<float>> defaultList = new();


        foreach (UserStoryAssociation userStoryAssociation in MyUserAssociation)
        {
            CreateDefaultListWithDays(defaultList);

            for (int i = 0; i < teamMembers.Count; i++)
            {
                var exists = userStoryAssociation.Days.FirstOrDefault(x => x.UserName == teamMembers[i].Username);
                if (exists != null)
                {
                    defaultList[i].UserName = exists.UserName;
                    defaultList[i].Value = exists.Value;
                }
                else
                {
                    defaultList[i].UserName = teamMembers[i].Username;
                    defaultList[i].Value = 0;
                }
            }
            userStoryAssociation.Days = new ObservableCollection<Wrapper<float>>(defaultList);
        }

        OrderTeamAndStoryInfo();

        SerializeStoryData();

        CalculateCoverage();

    }

    private void GetBusinessCaseForEpics()
    {
        foreach (var epic in Epics)
        {
            if (epic.BusinessCase != null)
            {
                businessCaseSet.Add(epic.BusinessCase);
            }
        }

        foreach (var businessCase in businessCaseSet)
        {
            BusinessCase.Add(businessCase);
        }
        BusinessCase.Add("PlaceHolder");
        BusinessCase.Add("None");
    }

    private List<Tuple<User, float>> GenerateDefaultDays()
    {
        List<Tuple<User, float>> capacityList = new();
        for (int i = 0; i < MaxNumberOfUsers; i++)
        {
            capacityList.Add(Tuple.Create(new User { Username = TeamMembers[i].Username, HasTeam = false }, 0f));
        }
        return capacityList;
    }

    private void ChangeColorByNumberOfDays()
    {
        for (int dayIndex = 0; dayIndex < Balancing[0].Days.Count; dayIndex++)
        {
            if (Balancing[0].Days[dayIndex].Value < 0)
                Balancing[0].ColorBackgroundList[dayIndex] = new SolidColorBrush(Colors.LightCoral);
            else if (Balancing[0].Days[dayIndex].Value < 4)
                Balancing[0].ColorBackgroundList[dayIndex] = new SolidColorBrush(Colors.Yellow);
            else
                Balancing[0].ColorBackgroundList[dayIndex] = new SolidColorBrush(Colors.LightGreen);
        }
    }

    private void DisplayStoriesFromAnEpic(int epicId)
    {
        MyUserAssociation.Clear();
        for (int allUserStoryAssociationIndex = 0; allUserStoryAssociationIndex < allUserStoryAssociation.Count; allUserStoryAssociationIndex++)
        {
            if (allUserStoryAssociation[allUserStoryAssociationIndex].StoryData.EpicID == epicId)
                MyUserAssociation.Add(allUserStoryAssociation[allUserStoryAssociationIndex]);
        }
    }

    public List<Tuple<User, float>> CalculateBalancing(bool shortTerm)
    {
        List<Tuple<User, float>> balance = new List<Tuple<User, float>>();
        var work = CalculateWork(shortTerm);
        var totalWork = GetTotalWork(shortTerm);
        for (int i = 0; i < work.Count; i++)
        {
            balance.Add(Tuple.Create(work[i].Item1, (float)Math.Round((work[i].Item2 - totalWork[i].Item2), 2)));
        }
        Balancing[0] = new UserStoryAssociation(
                 new IssueData("Balancing", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 balance,
                 MaxNumberOfUsers
             );
        ChangeColorByNumberOfDays();
        return balance;
    }
    public List<Tuple<User, float>> CalculateOpenTasks()
    {
        List<Tuple<User, float>> workOpenStory = new List<Tuple<User, float>>();
        foreach (var item in OpenTasks)
        {
            workOpenStory.Add(Tuple.Create(item.User, (float)Math.Round(item.Remaining, 2)));
        }
        workOpenStory.OrderBy(x => x.Item1.Username).ToList();
        return workOpenStory;
    }
    public List<Tuple<User, float>> CalculateWorkOpenstories(bool shortTerm)
    {
        List<Tuple<User, float>> openstories = new List<Tuple<User, float>>();
        if (shortTerm == false)
        {
            foreach (var member in TeamMembers)
            {
                float totalSum = 0;
                if (member.HasTeam)
                    foreach (var item in allUserStoryAssociation)
                    {
                        foreach (var day in item.Days)
                            if (member.Username == day.UserName)
                            {
                                totalSum += day.Value * item.Remaining;
                            }
                    }
                openstories.Add(Tuple.Create(member, (float)Math.Round(totalSum / 100, 2)));
            }
        }
        else
        {
            foreach (var member in TeamMembers)
            {
                float totalSum = 0;
                if (member.HasTeam)
                    foreach (var item in ShortTermStoryes)
                    {
                        foreach (var day in item.Days)
                            if (member.Username == day.UserName)
                            {
                                totalSum += day.Value * item.Remaining;
                            }
                    }
                openstories.Add(Tuple.Create(member, (float)Math.Round(totalSum / 100, 2)));
            }
        }

        return openstories;
    }
    public List<Tuple<User, float>> GetTotalWork(bool shortTerm)
    {
        List<Tuple<User, float>> totalWork = new List<Tuple<User, float>>();
        List<Tuple<User, float>> work = CalculateWorkOpenstories(shortTerm);
        List<Tuple<User, float>> openStories = CalculateOpenTasks();
        for (int i = 0; i < work.Count; i++)
        {
            totalWork.Add(Tuple.Create(openStories[i].Item1, (float)Math.Round(work[i].Item2 + openStories[i].Item2, 2)));
        }
        return totalWork;

    }
    public List<Tuple<User, float>> CalculateWork(bool shortTerm)
    {

        totalWork = new();

        int numberOfWorkingDays = 0;

        var vm = _serviceCollection.GetService(typeof(SprintSelectionViewModel));
        if (vm != null)
        {
            if (shortTerm)
            {
                numberOfWorkingDays = ((SprintSelectionViewModel)vm).GetWorkingDays();
            }
            else
            {
                numberOfWorkingDays = ((SprintSelectionViewModel)vm).RemainingDays();
            }
        }
        foreach (var item in TeamMembers)
        {
            totalWork.Add(Tuple.Create(item, (float)(numberOfWorkingDays)));
        }
        totalWork = totalWork.OrderBy(x => x.Item1.Username).ToList();
        return totalWork;
    }

    private void InitializeTotals()
    {

        totalWork = new List<Tuple<User, float>>(MaxNumberOfUsers);

        Totals[0] = new UserStoryAssociation(
                 new IssueData("Open tasks", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 OpenTasks.Select(obj => Tuple.Create(obj.User, obj.Remaining)).ToList(),
                 MaxNumberOfUsers
             );

        Totals[1] = new UserStoryAssociation(
              new IssueData("Total work open story", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
              true,
              3.0f,
              CalculateWorkOpenstories(IsShortTermVisible),
              MaxNumberOfUsers
          );

        Totals[2] = new UserStoryAssociation(
                  new IssueData("Total work", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 //we need a float list 
                 GetTotalWork(IsShortTermVisible),
                 MaxNumberOfUsers
             );

        Totals[3] = new UserStoryAssociation(
                 new IssueData("Total capacity", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story),
                 true,
                 3.0f,
                 CalculateWork(IsShortTermVisible),
                 MaxNumberOfUsers
             );
    }

    private void ChangeColorOnCovorage()
    {
        for(int asocIndex=0; asocIndex < allUserStoryAssociation.Count; asocIndex++)
        { 
            if (allUserStoryAssociation[asocIndex].Coverage.Value == 0 || allUserStoryAssociation[asocIndex].Coverage.Value >100)
                allUserStoryAssociation[asocIndex].ColorBackgroundList[0] = new SolidColorBrush(Colors.LightCoral);
            else if (allUserStoryAssociation[asocIndex].Coverage.Value == 100)
                allUserStoryAssociation[asocIndex].ColorBackgroundList[0] = new SolidColorBrush(Colors.LightGreen);
            else
                allUserStoryAssociation[asocIndex].ColorBackgroundList[0] = new SolidColorBrush(Colors.Yellow);
        }
    }

    private void RepopulateEpics()
    {
        Epics.Clear();
        List<IssueData> epics = _queriesForDataBase.GetAllEpicsByTeamLeader(SelectedUser);
        foreach (var item in epics)
        {
            Epics.Add(item);
        }
    }
    
    private void Filter()
    {
        if (FilterString == string.Empty) return;

        switch (FilterString)
        {

            case "None":
                RepopulateEpics();
                MyUserAssociation.Clear();
                foreach (UserStoryAssociation userStoryAssociation in allUserStoryAssociation)
                {
                    MyUserAssociation.Add(userStoryAssociation);
                }
                if (currentEpicId != -1)
                {
                    for (int userStoryAssociationIndex = 0; userStoryAssociationIndex < MyUserAssociation.Count; ++userStoryAssociationIndex)
                    {
                        if (MyUserAssociation[userStoryAssociationIndex].StoryData.EpicID != currentEpicId)
                        {
                            MyUserAssociation.Remove(MyUserAssociation[userStoryAssociationIndex]);
                            userStoryAssociationIndex--;
                        }
                    }
                }

                break;

            case "PlaceHolder":
                RepopulateEpics();
                MyUserAssociation.Clear();
                foreach (UserStoryAssociation userStoryAssociation in allUserStoryAssociation)
                {
                    MyUserAssociation.Add(userStoryAssociation);
                }
                for (int userStoryAssociationIndex = 0; userStoryAssociationIndex < MyUserAssociation.Count; ++userStoryAssociationIndex)
                {
                    if (!MyUserAssociation[userStoryAssociationIndex].StoryData.Name.Contains("#"))
                    {
                        MyUserAssociation.Remove(MyUserAssociation[userStoryAssociationIndex]);
                        userStoryAssociationIndex--;
                    }
                }
                break;
            

            default:
                RepopulateEpics();
                for (int issueIndex = 0; issueIndex < Epics.Count; issueIndex++)
                {
                    if (Epics[issueIndex].BusinessCase != FilterString)
                    {

                        if (MyUserAssociation.Count > 0 && MyUserAssociation.First().StoryData.EpicID == Epics[issueIndex].Id && MyUserAssociation.Count != allUserStoryAssociation.Count)
                            MyUserAssociation.Clear();

                        Epics.Remove(Epics[issueIndex]);
                        issueIndex--;
                    }

                    if(MyUserAssociation.Count == 0)
                    {
                        foreach(var asoc in allUserStoryAssociation)
                        {
                            MyUserAssociation.Add(asoc);
                        }
                    }
                }

                if (MyUserAssociation.Count == allUserStoryAssociation.Count)
                {
                    for (int userStoryAssociationIndex = 0; userStoryAssociationIndex < MyUserAssociation.Count; ++userStoryAssociationIndex)
                    {
                        int issueIndex;
                        for (issueIndex = 0; issueIndex < Epics.Count; issueIndex++)
                        {
                            if (MyUserAssociation[userStoryAssociationIndex].StoryData.EpicID == Epics[issueIndex].Id)
                                break;
                        }
                        if (issueIndex == Epics.Count)
                        {
                            MyUserAssociation.Remove(MyUserAssociation[userStoryAssociationIndex]);
                            userStoryAssociationIndex--;
                        }

                    }
                }
                break;


        };

    }

    public void RefreshStories()
    {
        List<Tuple<User, float>> capacityList = GenerateDefaultDays();

        foreach (var story in allStories)
        {
            if (allUserStoryAssociation.FirstOrDefault(u => u.StoryData.Id == story.Id) == null)
            {
                allUserStoryAssociation.Add(new UserStoryAssociation(story, false, story.Remaining, capacityList, MaxNumberOfUsers));
                MyUserAssociation.Add(allUserStoryAssociation.Last());
            }
        }
    }
    public void CalculateCoverage()
    {

        for (int i = 0; i < MyUserAssociation.Count; i++)
        {
            MyUserAssociation[i].CalculateCoverage();
        }

        ChangeColorOnCovorage();
    }

    public void CalculateTotals()
    {

        CalculateWork(IsShortTermVisible);
        OrderTeamAndStoryInfo();
        CalculateBalancing(IsShortTermVisible);
        InitializeTotals();
    }


    //RelayCommands
    [RelayCommand]
    public void OpenTeamPage()
    {
        if (SelectedUser != null)
        {
            var vm = _serviceCollection.GetService(typeof(TeamViewModel));
            if (vm != null)
            {
                ((TeamViewModel)vm).PopulateUsersLists(SelectedUser.Username);
            }
            _navigationService.CurrentPageType = typeof(TeamPage);
        }

    }

    [RelayCommand]
    public void OpenSprintSelectionPage()
    {

        if (SelectedUser != null)
        {
            IsBalancing = false;
            var vm= _serviceCollection.GetService(typeof(SprintSelectionViewModel));
            if (vm != null) 
            {
                ((SprintSelectionViewModel)vm).UpdateSprintShortTermInfo();
            }
            _navigationService!.CurrentPageType = typeof(SprintSelectionPage);
        }
    }

    [RelayCommand]
    public void SerializeOnSave()
    {
        if (SelectedUser == null)
        {
            return;
        }

        SerializeStoryData();


        var mainWindow = _serviceCollection.GetService(typeof(Window));
        var dialog = new SaveSuccessfulWindow("Saved succesfully!");
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialog.ShowDialog((MainWindow)mainWindow);
    }

    [RelayCommand]
    public void EpicClicked(int id)
    {
        SerializeStoryData();
        DisplayStoriesFromAnEpic(id);
        ShowShortTermStoryes();

        currentEpicId = id;
        GetStories = true;


        CalculateCoverage();
        
        OrderTeamAndStoryInfo();
        
    }

    [RelayCommand]
    public void AllEpicsClicked()
    {
        SerializeStoryData();
        allUserStoryAssociation.Clear();
        ShowAllStories();
        CalculateCoverage();
    }

    [RelayCommand]
    public void ShowAllStories()
    {
        MyUserAssociation.Clear();
        if (File.Exists(JsonSerialization.UserStoryFilePath + SelectedUser.Username))
        {
            GetSerializedData();
            CalculateCoverage();
        }
        else
        {
            PopulateByDefault();
        }
        ShowShortTermStoryes();
        GetStories = true;
        currentEpicId = -1;

        Filter();
        
    }

    [RelayCommand]
    public void RefreshClicked()
    {
        if (SelectedUser == null)
        {
            return;
        }
        MyUserAssociation.Clear();

        allUserStoryAssociation.Clear();

        allStories = _queriesForDataBase.GetAllStoriesByTeamLeader(SelectedUser);

        ShowAllStories();

        RefreshStories();

        OrderTeamAndStoryInfo();
    }

    [RelayCommand]
    public void CalculateCoverageButton()
    {
        if (SelectedUser != null)
        {
            CalculateCoverage();
            CalculateTotals();
        }
    }


    [RelayCommand]
    public void ShowShortTermStoryes()
    {
        if (SelectedUser == null)
        {
            IsShortTermVisible = false;
            return;
        }
        ShortTermStoryes = new();

        for (int i = 0; i < allUserStoryAssociation.Count; i++)
        {
            if (allUserStoryAssociation[i].ShortTerm)
            {
                ShortTermStoryes.Add(allUserStoryAssociation[i]);
            }
        }

        CalculateTotalsButton();
        IsBalancing = true;
    }

    [RelayCommand]
    public void CalculateTotalsButton()
    {
        if (SelectedUser == null)
        {
            IsBalancing = false;
            return;
        }

        var vm = _serviceCollection.GetService(typeof(SprintSelectionViewModel));
        if (vm != null)
        {
            if (((SprintSelectionViewModel)vm).Sprints.Count == 0)
            {
                //var mainWindow = _serviceCollection.GetService(typeof(Window));
                //var dialog = new SaveSuccessfulWindow("Define your sprints first!");
                //dialog.Title = "Info";
                //dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                //dialog.ShowDialog((MainWindow)mainWindow);

                IsBalancing = false;
                return;
            }
        }

        CalculateTotals();
    }

    [RelayCommand]
    public void UncheckShortTermStory(string id)
    {
        for (int i = 0; i < MyUserAssociation.Count; i++)
        {
            if (MyUserAssociation[i].StoryData.Name == id)
            {
                MyUserAssociation[i].ShortTerm = false;
                ShortTermStoryes.Remove(MyUserAssociation[i]);
            }
        }
    }

    [RelayCommand]
    public void DeleteLocalFiles()
    {
        if (SelectedUser == null)
        {
            return;
        }
     
        var mainWindow = _serviceCollection.GetService(typeof(Window));
        var dialog = new DeleteLocalFilesWindow("Do you want to delete local files?");
        dialog.SelectedUser = SelectedUser;
        dialog.serviceCollection = _serviceCollection;
        dialog.Title = "Delete Local Files";
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialog.ShowDialog((MainWindow)mainWindow);

    }

    [RelayCommand]
    public void OpenReleaseCalendar()
    {
        if (SelectedUser != null)
        {
            var vm = _serviceCollection.GetService(typeof(ReleaseCalendarViewModel));
            if (vm != null)
            {
                ((ReleaseCalendarViewModel)vm).GetSprintsFromSprintSelection();
            }

            _navigationService.CurrentPageType = typeof(ReleaseCalendarPage);
        }
    }
    
    
    
};




