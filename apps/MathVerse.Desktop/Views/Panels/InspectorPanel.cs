using System.Linq;
using Avalonia.Controls;
using MathVerse.Desktop.Core;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop;

public partial class InspectorPanel : UserControl
{
    public InspectorPanel()
    {
        InitializeComponent();
        AppServices.EventBus.Subscribe(EventType.ObjectSelectionChanged, OnSelectionChanged);
    }

    private void OnSelectionChanged(EventData data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var selected = AppServices.SelectionManager.SelectedObjects.ToList();

            if (selected.Count == 0)
            {
                NoSelectionText.IsVisible = true;
                PropertyPanel.IsVisible = false;
                return;
            }

            NoSelectionText.IsVisible = false;
            PropertyPanel.IsVisible = true;

            var obj = selected[0];
            ObjectName.Text = obj.Name;
            ObjectType.Text = obj.TypeTag;
            ObjectId.Text = obj.Id.ToString("N")[..8];
            ObjectVisible.Text = obj.IsVisible ? "Yes" : "No";
            ObjectExpression.Text = obj.Metadata.TryGetValue("Expression", out var expr) ? expr?.ToString() ?? "" : "";
        });
    }
}
