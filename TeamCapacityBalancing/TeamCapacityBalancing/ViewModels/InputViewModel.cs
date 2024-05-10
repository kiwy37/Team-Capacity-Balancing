using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamCapacityBalancing.Navigation;

namespace TeamCapacityBalancing.ViewModels;

public sealed partial class InputViewModel : ObservableObject
{
    public ServiceCollection? _serviceCollection;

    [ObservableProperty]
    private int _hours;

    [ObservableProperty]
    private string _currentUsername;

    public InputViewModel()
    {
        _currentUsername = string.Empty;
    }

    public InputViewModel(ServiceCollection serviceCollection)
    {
        _currentUsername = string.Empty;
        _serviceCollection = serviceCollection;
        var vm = _serviceCollection.GetService(typeof(TeamViewModel));
        
        if (vm != null)   //TODO: solve this warning if somebody knows how to do it
        {
            CurrentUsername=((TeamViewModel)vm).SelectedUserYourTeam.DisplayName;
            Hours =(int)((TeamViewModel)vm).SelectedUserYourTeam.HoursPerDay.Value;
        }

    }

    [RelayCommand]
    public void AtClose()
    {
        if (_serviceCollection != null)
        {
            var vm = _serviceCollection.GetService(typeof(TeamViewModel));
            if (vm != null)
            {
                ((TeamViewModel)vm).SelectedUserYourTeam.HoursPerDay.Value = Hours;
            }
        }
    }
}
