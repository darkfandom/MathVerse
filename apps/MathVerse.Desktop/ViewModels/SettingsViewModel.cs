using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private double _uiScale = 1.0;
    [ObservableProperty] private bool _gpuAcceleration = true;
}
