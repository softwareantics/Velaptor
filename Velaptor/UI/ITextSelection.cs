namespace Velaptor.UI;

using System;
using System.Drawing;
using System.Numerics;
using System.Text;
using Graphics;
using Input;

public interface ITextSelection
{
    bool InSelectionMode { get; set; }

    RectShape SelectionRect { get; }

    Vector2 Position { get; set; }

    Color SelectionColor { get; set; }

    int SelectionStartIndex { get; set; }

    int SelectionStopIndex { get; set; }

    string SelectedText { get; }

    int Height { get; set; }

    RectangleF TextViewArea { get; set; }

    void Clear();

    void UpdateSelectionRect(KeyCode key, RectangleF[] charBounds, RectShape cursorBounds, StringBuilder text);
}
