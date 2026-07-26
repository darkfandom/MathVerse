using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MathVerse.Desktop.ViewModels;

public partial class WorkspaceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableObject _currentPage;
    [ObservableProperty] private string _currentTitle = "Home";
    [ObservableProperty] private string _currentPageName = "home";
    [ObservableProperty] private bool _canGoBack;
    [ObservableProperty] private bool _canGoForward;

    private readonly List<string> _backStack = new();
    private readonly List<string> _forwardStack = new();
    private bool _isNavigating;

    public HomeViewModel Home { get; }
    public EvaluateViewModel Evaluate { get; }
    public GraphViewModel Graph { get; }
    public SettingsViewModel Settings { get; }

    public WorkspaceViewModel(
        HomeViewModel home,
        EvaluateViewModel evaluate,
        GraphViewModel graph,
        SettingsViewModel settings)
    {
        Home = home;
        Evaluate = evaluate;
        Graph = graph;
        Settings = settings;
        _currentPage = home;
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        if (_isNavigating) return;
        if (page == CurrentPageName) return;

        _isNavigating = true;
        _backStack.Add(CurrentPageName);
        _forwardStack.Clear();
        SetPage(page);
        _isNavigating = false;
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_backStack.Count == 0) return;
        _isNavigating = true;
        _forwardStack.Add(CurrentPageName);
        var page = _backStack[^1];
        _backStack.RemoveAt(_backStack.Count - 1);
        SetPage(page);
        _isNavigating = false;
    }

    [RelayCommand]
    private void GoForward()
    {
        if (_forwardStack.Count == 0) return;
        _isNavigating = true;
        _backStack.Add(CurrentPageName);
        var page = _forwardStack[^1];
        _forwardStack.RemoveAt(_forwardStack.Count - 1);
        SetPage(page);
        _isNavigating = false;
    }

    private void SetPage(string page)
    {
        CurrentPageName = page;
        switch (page)
        {
            case "home": CurrentPage = Home; CurrentTitle = "Home"; break;
            case "evaluate": CurrentPage = Evaluate; CurrentTitle = "Evaluate"; break;
            case "graph": CurrentPage = Graph; CurrentTitle = "Graph Studio"; break;
            case "settings": CurrentPage = Settings; CurrentTitle = "Settings"; break;
            default: CurrentPage = Home; CurrentTitle = "Home"; break;
        }
        CanGoBack = _backStack.Count > 0;
        CanGoForward = _forwardStack.Count > 0;
    }

    public void ClearHistory()
    {
        _backStack.Clear();
        _forwardStack.Clear();
        CanGoBack = false;
        CanGoForward = false;
    }
}
