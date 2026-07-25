using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class VisualizeViewModel : ObservableObject
{
    [ObservableProperty] private string _activeCategory = "Calculus";
}
