using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Services;

public sealed class ObjectFactory
{
    public IWorkspaceObject CreateExpression(string expression)
    {
        return new GenericWorkspaceObject("Expression", expression)
        {
            Category = "Mathematics"
        };
    }

    public IWorkspaceObject CreateGraph(string expression, string type = "Cartesian", string color = "#4488FF")
    {
        var obj = new GenericWorkspaceObject("Graph", expression)
        {
            Category = "Visualization"
        };
        obj.Metadata["Expression"] = expression;
        obj.Metadata["GraphType"] = type;
        obj.Metadata["Color"] = color;
        return obj;
    }

    public IWorkspaceObject CreateSurface(string expression)
    {
        var obj = new GenericWorkspaceObject("Surface", expression)
        {
            Category = "Visualization"
        };
        obj.Metadata["Expression"] = expression;
        return obj;
    }

    public IWorkspaceObject CreateGeometry(string type)
    {
        return new GenericWorkspaceObject("Geometry", type)
        {
            Category = "Geometry"
        };
    }

    public IWorkspaceObject CreateSimulation(string type)
    {
        return new GenericWorkspaceObject("Simulation", type)
        {
            Category = "Simulation"
        };
    }

    public IWorkspaceObject CreateDataset(string name)
    {
        return new GenericWorkspaceObject("Dataset", name)
        {
            Category = "Data"
        };
    }

    public IWorkspaceObject CreateNotebook(string name)
    {
        return new GenericWorkspaceObject("Notebook", name)
        {
            Category = "Research"
        };
    }

    public IWorkspaceObject CreateScene(string name)
    {
        return new GenericWorkspaceObject("Scene", name)
        {
            Category = "Scene"
        };
    }

    public IWorkspaceObject CreateCamera(string name)
    {
        return new GenericWorkspaceObject("Camera", name)
        {
            Category = "Viewport"
        };
    }

    public IWorkspaceObject CreateLight(string name)
    {
        return new GenericWorkspaceObject("Light", name)
        {
            Category = "Viewport"
        };
    }
}
