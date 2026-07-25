using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public partial class AnimateViewModel : ObservableObject
{
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private double _currentFrame;
}
