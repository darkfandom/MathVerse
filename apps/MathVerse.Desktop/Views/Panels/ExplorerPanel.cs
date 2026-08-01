using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop;

public partial class ExplorerPanel : UserControl
{
    private static readonly Dictionary<string, Color> TypeColors = new()
    {
        ["Expression"] = Color.FromRgb(0x4A, 0x9E, 0xFF),
        ["Graph"] = Color.FromRgb(0x4C, 0xAF, 0x50),
        ["Surface"] = Color.FromRgb(0xFF, 0x98, 0x00),
        ["Geometry"] = Color.FromRgb(0xE0, 0x40, 0x40),
        ["Dataset"] = Color.FromRgb(0xAB, 0x47, 0xBC),
        ["Text"] = Color.FromRgb(0x78, 0x90, 0x9C),
        ["Folder"] = Color.FromRgb(0xFF, 0xCA, 0x28),
    };

    public ExplorerPanel()
    {
        InitializeComponent();
        var bus = AppServices.EventBus;
        bus.Subscribe(EventType.ObjectCreated, _ => Rebuild());
        bus.Subscribe(EventType.ObjectDeleted, _ => Rebuild());
        bus.Subscribe(EventType.ObjectPropertyChanged, _ => Rebuild());
        bus.Subscribe(EventType.ObjectSelectionChanged, _ => UpdateSelectionHighlight());
        bus.Subscribe(EventType.ActiveObjectChanged, _ => UpdateSelectionHighlight());
    }

    private void Rebuild()
    {
        Dispatcher.UIThread.Post(() =>
        {
            ObjectContainer.Children.Clear();
            var objects = AppServices.Registry.GetAll().ToList();
            var hasObjects = objects.Count > 0;

            EmptyState.IsVisible = !hasObjects;
            ObjectScroller.IsVisible = hasObjects;

            if (!hasObjects) return;

            var grouped = objects
                .GroupBy(o => o.TypeTag)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                ObjectContainer.Children.Add(CreateGroupHeader(group.Key, group.Count()));
                foreach (var obj in group.OrderBy(o => o.Name))
                    ObjectContainer.Children.Add(CreateObjectRow(obj));
            }
        });
    }

    private Border CreateGroupHeader(string type, int count)
    {
        return new Border
        {
            Padding = new Avalonia.Thickness(8, 4, 8, 2),
            Child = new TextBlock
            {
                Text = $"{type.ToUpperInvariant()} ({count})",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
            }
        };
    }

    private Border CreateObjectRow(IWorkspaceObject obj)
    {
        var color = TypeColors.GetValueOrDefault(obj.TypeTag, Color.FromRgb(0x78, 0x90, 0x9C));
        var isSelected = AppServices.SelectionService.IsSelected(obj.Id);
        var isActive = AppServices.SelectionService.ActiveObjectId == obj.Id;

        var dot = new Avalonia.Controls.Shapes.Ellipse
        {
            Width = 10, Height = 10,
            Fill = new SolidColorBrush(color),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
        Grid.SetColumn(dot, 0);

        var nameText = new TextBlock
        {
            Text = obj.Name,
            FontSize = 12,
            Foreground = new SolidColorBrush(isActive ? Color.FromRgb(0xFF, 0xFF, 0xFF) : Color.FromRgb(0xCC, 0xCC, 0xCC)),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(6, 0, 0, 0),
        };
        Grid.SetColumn(nameText, 1);

        var visBtn = CreateIconButton("\u25C9", obj.IsVisible ? "#666" : "#333", () => ToggleVisibility(obj));
        Grid.SetColumn(visBtn, 2);

        var delBtn = CreateIconButton("\u2715", "#666", () => DeleteObject(obj));
        Grid.SetColumn(delBtn, 3);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(16)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(22)),
                new ColumnDefinition(new GridLength(22)),
            },
            Height = 24,
        };
        grid.Children.Add(dot);
        grid.Children.Add(nameText);
        grid.Children.Add(visBtn);
        grid.Children.Add(delBtn);

        var bg = isSelected
            ? (IBrush)new SolidColorBrush(Color.FromRgb(0x2D, 0x5F, 0x8A))
            : Brushes.Transparent;

        var row = new Border
        {
            Padding = new Avalonia.Thickness(4, 2),
            Background = bg,
            Child = grid,
        };

        row.PointerPressed += (_, e) =>
        {
            var modifiers = e.KeyModifiers;
            if (modifiers.HasFlag(KeyModifiers.Control))
                AppServices.CommandManager.Execute("ToggleSelectObject",
                    new Dictionary<string, object> { ["ObjectId"] = obj.Id });
            else if (modifiers.HasFlag(KeyModifiers.Shift))
                AppServices.CommandManager.Execute("ToggleSelectObject",
                    new Dictionary<string, object> { ["ObjectId"] = obj.Id });
            else
                AppServices.CommandManager.Execute("SelectObject",
                    new Dictionary<string, object> { ["ObjectId"] = obj.Id });
        };

        return row;
    }

    private void UpdateSelectionHighlight()
    {
        Dispatcher.UIThread.Post(() => Rebuild());
    }

    private Border CreateIconButton(string text, string color, Action onClick)
    {
        var btn = new Border
        {
            Width = 18, Height = 18,
            Background = Brushes.Transparent,
            CornerRadius = new Avalonia.CornerRadius(3),
            Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.Parse(color)),
                FontSize = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            }
        };
        btn.PointerPressed += (_, _) => onClick();
        return btn;
    }

    private void ToggleVisibility(IWorkspaceObject obj)
    {
        obj.IsVisible = !obj.IsVisible;
        AppServices.EventBus.Publish(new EventData(EventType.ObjectPropertyChanged, obj.Id, "IsVisible"));
    }

    private void DeleteObject(IWorkspaceObject obj)
    {
        AppServices.Registry.Remove(obj.Id);
        AppServices.Workspace.RemoveObject(obj.Id);
        if (AppServices.SelectionService.IsSelected(obj.Id))
            AppServices.SelectionService.Deselect(obj.Id);
        AppServices.EventBus.Publish(new EventData(EventType.ObjectDeleted, obj.Id));
    }
}
