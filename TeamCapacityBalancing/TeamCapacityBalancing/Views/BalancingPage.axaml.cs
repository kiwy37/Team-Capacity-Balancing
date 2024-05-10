using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TeamCapacityBalancing.Models;
using TeamCapacityBalancing.Services.Postgres_connection;
using TeamCapacityBalancing.ViewModels;

namespace TeamCapacityBalancing.Views;

public partial class BalancingPage : UserControl
{
    public BalancingPage()
    {
        InitializeComponent();
        dataGridZero.SelectionChanged += (sender, e) =>
        {
            dataGridZero.SelectedItems.Clear();
        };
        dataGridOne.SelectionChanged += (sender, e) =>
        {
            dataGridOne.SelectedItems.Clear();
        };
        dataGridTwo.SelectionChanged += (sender, e) =>
        {
            dataGridTwo.SelectedItems.Clear();
        };
        dataGridTree.SelectionChanged += (sender, e) =>
        {
            dataGridTree.SelectedItems.Clear();
        };
    }
}
