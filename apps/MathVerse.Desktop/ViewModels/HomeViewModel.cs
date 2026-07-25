using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] private string _welcomeMessage = "Welcome to MathVerse";
    [ObservableProperty] private string _statusSummary = "Your scientific mathematics platform";
}
