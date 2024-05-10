using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCapacityBalancing.Navigation;
using TeamCapacityBalancing.Views;

namespace TeamCapacityBalancing.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    private readonly NavigationService? _navigationService;
    private readonly ServiceCollection _serviceCollection;

    public HomeViewModel(NavigationService navigationService, ServiceCollection serviceCollection)
    {
        _navigationService = navigationService;
        _serviceCollection = serviceCollection;
    }

    [RelayCommand]
    public void OpenBalancigPage()
    {
        _navigationService!.CurrentPageType = typeof(BalancingPage);
    }
}
