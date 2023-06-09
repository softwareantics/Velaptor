namespace Velaptor.UI;

using System.Drawing;
using Graphics;

internal interface ITextSelection
{
    RectShape SelectionRect { get; }

    string SelectedText { get; }

    Color SelectionColor { get; set; }

    void Update();

    void Clear();
}
