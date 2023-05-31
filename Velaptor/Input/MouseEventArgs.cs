namespace Velaptor.Input;

using System.Diagnostics.CodeAnalysis;

public readonly record struct MouseEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MouseEventArgs"/> class.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1642:Constructor summary documentation should begin with standard text",
        Justification = "Reported incorrectly.  This is a struct, not a class")]
    public MouseEventArgs(int x, int y, MouseButton button)
    {
        // TODO: Add scroll wheel value
    }
}
