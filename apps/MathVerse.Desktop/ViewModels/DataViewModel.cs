using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class DataViewModel : ObservableObject
{
    [ObservableProperty] private string _dataSource = string.Empty;
}
