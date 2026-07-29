using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Rendering;

public interface IRenderCompiler
{
    string TypeTag { get; }
    bool CanCompile(IWorkspaceObject obj);
    IRenderObject[] Compile(IWorkspaceObject obj);
}
