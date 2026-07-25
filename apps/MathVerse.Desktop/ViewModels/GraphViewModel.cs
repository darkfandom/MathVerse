using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class GraphViewModel : ObservableObject
{
    [ObservableProperty] private string _expressionInput = string.Empty;
    [ObservableProperty] private string _plotType = "2D";
}
