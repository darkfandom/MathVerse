using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class SimulateViewModel : ObservableObject
{
    [ObservableProperty] private string _simulationType = "Physics";
    [ObservableProperty] private bool _isRunning;
}
