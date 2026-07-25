using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MathVerse.Desktop.ViewModels;

public partial class WorkspaceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableObject _currentPage;
    [ObservableProperty] private string _currentTitle = "Home";
    [ObservableProperty] private string _currentPageName = "home";

    public HomeViewModel Home { get; }
    public EvaluateViewModel Evaluate { get; }
    public GraphViewModel Graph { get; }
    public VisualizeViewModel Visualize { get; }
    public GeometryViewModel Geometry { get; }
    public SimulateViewModel Simulate { get; }
    public AnimateViewModel Animate { get; }
    public AiViewModel Ai { get; }
    public PublishViewModel Publish { get; }
    public DataViewModel Data { get; }
    public SettingsViewModel Settings { get; }

    public WorkspaceViewModel(
        HomeViewModel home,
        EvaluateViewModel evaluate,
        GraphViewModel graph,
        VisualizeViewModel visualize,
        GeometryViewModel geometry,
        SimulateViewModel simulate,
        AnimateViewModel animate,
        AiViewModel ai,
        PublishViewModel publish,
        DataViewModel data,
        SettingsViewModel settings)
    {
        Home = home;
        Evaluate = evaluate;
        Graph = graph;
        Visualize = visualize;
        Geometry = geometry;
        Simulate = simulate;
        Animate = animate;
        Ai = ai;
        Publish = publish;
        Data = data;
        Settings = settings;
        _currentPage = home;
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        CurrentPageName = page;
        switch (page)
        {
            case "home": CurrentPage = Home; CurrentTitle = "Home"; break;
            case "evaluate": CurrentPage = Evaluate; CurrentTitle = "Evaluate"; break;
            case "graph": CurrentPage = Graph; CurrentTitle = "Graph Studio"; break;
            case "visualize": CurrentPage = Visualize; CurrentTitle = "Visualization Studio"; break;
            case "geometry": CurrentPage = Geometry; CurrentTitle = "Geometry Studio"; break;
            case "simulate": CurrentPage = Simulate; CurrentTitle = "Simulation Lab"; break;
            case "animate": CurrentPage = Animate; CurrentTitle = "Animation Studio"; break;
            case "ai": CurrentPage = Ai; CurrentTitle = "AI Assistant"; break;
            case "publish": CurrentPage = Publish; CurrentTitle = "Publications"; break;
            case "data": CurrentPage = Data; CurrentTitle = "Data Analysis"; break;
            case "settings": CurrentPage = Settings; CurrentTitle = "Settings"; break;
            default: CurrentPage = Home; CurrentTitle = "Home"; break;
        }
    }
}
