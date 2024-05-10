
﻿using Avalonia.Media;
﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TeamCapacityBalancing.Services;

namespace TeamCapacityBalancing.Models;

public class Wrapper<T> : Utility
{
    public T? _value;
    public T? Value
    {
        get => _value;
        set
        {
            _value = value;
            NotifyPropertyChanged();
        }
    }

    public string UserName { get; set; }

    public Wrapper()
    {
    }
    public T? GetValue()
    {
        return Value;
    }
}

public partial class UserStoryAssociation : ObservableObject
{
    public IssueData StoryData { get; set; }
    public float Remaining { get; set; }
    public Wrapper<float> Coverage { get; set; }


    ObservableCollection<Avalonia.Media.Brush> _colorBackgroundList;
    public ObservableCollection<Avalonia.Media.Brush> ColorBackgroundList
    {
        get => _colorBackgroundList;
        set
        {
            _colorBackgroundList = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Wrapper<float>> _days;
    public ObservableCollection<Wrapper<float>> Days
    {
        get => _days;
        set
        {
            _days = value;
            OnPropertyChanged();
        }
    }


    [ObservableProperty]
    public bool _shortTerm;

    

    public UserStoryAssociation(IssueData storyData, bool shortTerm, float remaining, List<Tuple<User, float>> days, int maxNumberOfUsers)
    {
        StoryData = storyData;
        ShortTerm = shortTerm;
        Remaining = remaining;
        _days = new(days.Select(x => new Wrapper<float>() { Value = x.Item2, UserName = x.Item1.Username }));
        _colorBackgroundList = new ObservableCollection<Avalonia.Media.Brush>(Enumerable.Repeat(new SolidColorBrush(Colors.White), maxNumberOfUsers).ToList());
        Coverage = new Wrapper<float>() { Value = 0 };
    }

    public UserStoryAssociation(IssueData storyData, bool shortTerm, float remaining, List<float> days, int maxNumberOfUsers) //After sebi does what he does it can die
    {
        StoryData = storyData;
        ShortTerm = shortTerm;
        Remaining = remaining;
        _days = new(days.Select(x => new Wrapper<float>() { Value = x, UserName = "default" }));
        _colorBackgroundList = new ObservableCollection<Avalonia.Media.Brush>(Enumerable.Repeat(new SolidColorBrush(Colors.White), maxNumberOfUsers).ToList());
        Coverage = new Wrapper<float>() { Value = 0 };
    }

    public void CalculateCoverage()
    {
        Coverage.Value = Days.Sum(x => x.Value);
    }
}
