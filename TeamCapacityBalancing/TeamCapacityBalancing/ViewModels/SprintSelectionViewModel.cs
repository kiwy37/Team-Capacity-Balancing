using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TeamCapacityBalancing.Models;
using TeamCapacityBalancing.Navigation;
using TeamCapacityBalancing.Services.LocalDataSerialization;
using TeamCapacityBalancing.Services.Postgres_connection;
using TeamCapacityBalancing.Services.ServicesAbstractions;
using TeamCapacityBalancing.Views;

namespace TeamCapacityBalancing.ViewModels;

public sealed partial class SprintSelectionViewModel : ObservableObject
{
    private readonly NavigationService? _navigationService;
    private readonly ServiceCollection? _serviceCollection;
    private int workingDays;

    public int GetWorkingDays()
    {
        return workingDays;
    }

    private readonly IDataSerialization _jsonSerialization = new JsonSerialization();
    public SprintSelectionViewModel()
    {

    }
    public SprintSelectionViewModel(NavigationService navigationService, ServiceCollection serviceCollection)
    {
        _navigationService = navigationService;
        _serviceCollection = serviceCollection;

        var vm = _serviceCollection.GetService(typeof(BalancingViewModel));
        if (vm != null)
        {
            UserNamePath = ((BalancingViewModel)vm).SelectedUser.Username;
            Sprints = new ObservableCollection<Sprint>(_jsonSerialization.DeserializeSprint(UserNamePath));
        }
        NrGenerateSprints = Sprints.Count;
    }

    public string UserNamePath { get; set; }

    [ObservableProperty]
    private int _nrGenerateSprints;
   

    [ObservableProperty]
    public bool _selectByEndDate=false;

    [ObservableProperty]
    public bool _selectByNrSprints=false;

    public ObservableCollection<Sprint> Sprints { get; set; } = new ObservableCollection<Sprint>()
    { };
    private int CalculcateWorkingDays(DateTime start, DateTime end)
    {
        int workingDays = 0;
        //check if the start and end are the same dates
        while (start.Date <= end.Date)
        {
            if (start.DayOfWeek != DayOfWeek.Saturday && start.DayOfWeek != DayOfWeek.Sunday)
            {
                workingDays++;
            }
            start = start.AddDays(1);
        }
        return workingDays;
    }
    public int RemainingDays()
    {
        if (Sprints.Count == 0)
        {
            return 0;
        }
        DateTime today = DateTime.Now;
        DateTime beginingOfSprint = DateTime.Parse(Sprints[0].StartDate);
        DateTime lastDate = DateTime.Parse(Sprints[Sprints.Count - 1].EndDate);
        if (today > beginingOfSprint)
        {
            return CalculcateWorkingDays(today, lastDate);
        }
        else
        {
            return CalculcateWorkingDays(beginingOfSprint, lastDate);
        }
    }

    [ObservableProperty]
    public DateTimeOffset? _finishDate;

    private int _nrSprints=0;

    public bool LostFocus { get; set; } = false;
 
    public int NrSprints 
    {
        get => _nrSprints;
        set 
        {
            _nrSprints = value;
            OnPropertyChanged(); 

            CalculateSprintData();

            for (int i = 0; i < Sprints.Count; i++)
            {
                if (DateTime.Parse(Sprints[i].EndDate) >= DateTime.Now && DateTime.Parse(Sprints[i].StartDate) <= DateTime.Now)
                {
                    if (i + NrSprints >= Sprints.Count)
                    {
                        FinishDate = DateTime.Parse(Sprints[Sprints.Count -1].EndDate);
                        //workingDays = CalculcateWorkingDays(DateTime.Now, FinishDate.GetValueOrDefault().DateTime);
                    }
                    else if(NrSprints != 0)
                    {
                        FinishDate = DateTime.Parse(Sprints[(NrSprints + i) - 1].EndDate);
                        //workingDays = CalculcateWorkingDays(DateTime.Now, FinishDate.GetValueOrDefault().DateTime);
                    }
                }
            }
        }
    }

    [ObservableProperty]
    public DateTimeOffset? _startDate=DateTimeOffset.Now;

    [ObservableProperty]
    public bool _selecteSprintForShortTerm = false;

    [RelayCommand]
    public void GenerateSprints()
    {
        Sprints.Clear();
        for (int i = 0; i < NrGenerateSprints; i++)
        {
            Sprints.Add(new Sprint($"Sprint {i + 1}", 3, false));
        }
    }

    [RelayCommand]
    public void OpenBalancigPage()
    {
        CalculateSprintData();

        if (_serviceCollection is not null)
        {
            var vm = _serviceCollection.GetService(typeof(BalancingViewModel));
            if (SelecteSprintForShortTerm)
            {
                if (vm != null)
                {
                    //((BalancingViewModel)vm).TotalWorkInShortTerm = totalWeeks * 5;
                }
            }
            else
            {
                if (vm != null && FinishDate is not null)
                {
                    //((BalancingViewModel)vm).finishDate = DateOnly.FromDateTime(FinishDate.Value.Date);
                }
            }
            if (vm != null)
            {

                List<Sprint> serializeSprint = new List<Sprint>(Sprints);
                _jsonSerialization.SerializeSprintData(serializeSprint, ((BalancingViewModel)vm).SelectedUser.Username);

            }

            if (SelectByEndDate)
            {
                DateTime dateTime = FinishDate.GetValueOrDefault().DateTime;
                workingDays = CalculcateWorkingDays(DateTime.Now, dateTime);
                SelectByNrSprints = false;
            }
            else if (SelectByNrSprints)
            {
                SelectByEndDate = false;
                for (int i = 0; i < Sprints.Count; i++)
                {
                    if (DateTime.Parse(Sprints[i].EndDate) >= DateTime.Now && DateTime.Parse(Sprints[i].StartDate) <= DateTime.Now)
                    {
                        if (i + NrSprints >= Sprints.Count)
                        {
                            FinishDate = DateTime.Parse(Sprints[Sprints.Count - 1].EndDate);
                            workingDays = CalculcateWorkingDays(DateTime.Now, FinishDate.GetValueOrDefault().DateTime);
                        }
                        else if (NrSprints != 0)
                        {
                            FinishDate = DateTime.Parse(Sprints[(NrSprints + i) - 1].EndDate);
                            workingDays = CalculcateWorkingDays(DateTime.Now, FinishDate.GetValueOrDefault().DateTime);
                        }
                    }
                }
            }

            _jsonSerialization.SerializeSelectionShortTermInfo(new SprintSelectionShortTerm(SelectByEndDate, SelectByNrSprints, NrSprints), UserNamePath);

            _navigationService!.CurrentPageType = typeof(BalancingPage);
        }
    }

    [RelayCommand]
    public void UncheckEndDate()
    {
        SelectByEndDate = true;
        SelectByNrSprints = false;
    }

    [RelayCommand]
    public void UncheckNrSprints() 
    {
        SelectByEndDate = false;
        SelectByNrSprints = true;
    }

    public void UpdateSprintShortTermInfo()
    {

        SprintSelectionShortTerm sprintSelectionShortTerm = _jsonSerialization.DeserializeSelectionShortTerm(UserNamePath);

        SelectByEndDate = sprintSelectionShortTerm.SelectByEndDate;
        SelectByNrSprints = sprintSelectionShortTerm.SelectByNrSprints;
        NrSprints = sprintSelectionShortTerm.NumberofSprints;
    }


    public void CalculateSprintData() 
    {
        float totalWeeks = 0;

        DateTime dueStart = StartDate.Value.DateTime;
        while (dueStart.DayOfWeek != DayOfWeek.Monday)
        {
            dueStart = dueStart.AddDays(-1);
        }
        for (int i = 0; i < Sprints.Count; i++)
        {
            Sprints[i].StartDate = dueStart.ToString("MM-dd-yyyy");
            dueStart = dueStart.AddDays(Sprints[i].NumberOfWeeks * 7);
            while (dueStart.DayOfWeek != DayOfWeek.Friday)
            {
                dueStart = dueStart.AddDays(-1);
            }
            Sprints[i].EndDate = dueStart.ToString("MM-dd-yyyy");
            dueStart = dueStart.AddDays(+3);
        }
        for (int i = 0; i < Sprints.Count; i++)
        {
            if (Sprints[i].IsInShortTerm)
            {
                totalWeeks = totalWeeks + Sprints[i].NumberOfWeeks;
            }
        }
    }
}
