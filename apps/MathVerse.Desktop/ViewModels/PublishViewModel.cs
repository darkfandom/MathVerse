using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class PublishViewModel : ObservableObject
{
    [ObservableProperty] private string _selectedTemplate = "Article";
}
