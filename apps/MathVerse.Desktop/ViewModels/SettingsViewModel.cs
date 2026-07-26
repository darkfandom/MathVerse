using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MathVerse.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private double _uiScale = 1.0;
    [ObservableProperty] private bool _gpuAcceleration = true;
    [ObservableProperty] private string _theme = "Dark";
    [ObservableProperty] private int _selectedThemeIndex;

    public double[] UiScales { get; } = [0.75, 1.0, 1.25, 1.5, 2.0];

    [RelayCommand]
    private void ResetDefaults()
    {
        UiScale = 1.0;
        GpuAcceleration = true;
        Theme = "Dark";
        SelectedThemeIndex = 0;
    }
}
