using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class GeometryViewModel : ObservableObject
{
    [ObservableProperty] private string _activeTool = "Select";
}
