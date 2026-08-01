using System.Linq;
using Avalonia.Controls;
using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop;

public partial class InspectorPanel : UserControl
{
    public InspectorPanel()
    {
        InitializeComponent();
        AppServices.EventBus.Subscribe(EventType.ObjectSelectionChanged, OnSelectionChanged);
        AppServices.EventBus.Subscribe(EventType.ActiveObjectChanged, OnSelectionChanged);
    }

    private void OnSelectionChanged(EventData data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var selected = AppServices.SelectionService.SelectedObjects.ToList();

            if (selected.Count == 0)
            {
                NoSelectionText.IsVisible = true;
                PropertyPanel.IsVisible = false;
                return;
            }

            NoSelectionText.IsVisible = false;
            PropertyPanel.IsVisible = true;

            if (selected.Count == 1)
            {
                var obj = selected[0];
                ObjectName.Text = obj.Name;
                ObjectType.Text = obj.TypeTag;
                ObjectId.Text = obj.Id.ToString("N")[..8];
                ObjectVisible.Text = obj.IsVisible ? "Yes" : "No";
                ObjectExpression.Text = obj.Metadata.TryGetValue("Expression", out var expr) ? expr?.ToString() ?? "" : "";
            }
            else
            {
                // Multi-selection: show shared properties summary
                ObjectName.Text = $"{selected.Count} objects selected";
                ObjectType.Text = string.Join(", ", selected.Select(o => o.TypeTag).Distinct());
                ObjectId.Text = "—";
                ObjectVisible.Text = selected.All(o => o.IsVisible) ? "Yes" :
                    selected.Any(o => o.IsVisible) ? "Mixed" : "No";
                ObjectExpression.Text = "—";
            }
        });
    }
}
