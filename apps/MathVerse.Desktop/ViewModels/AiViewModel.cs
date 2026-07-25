using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class AiViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = string.Empty;
}
