using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class EvaluateViewModel : ObservableObject
{
    [ObservableProperty] private string _expressionInput = string.Empty;
    [ObservableProperty] private string _expressionResult = string.Empty;
}
