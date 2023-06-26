namespace Velaptor.ReactableData;

using System.Drawing;
using System.Numerics;
using System.Text;
using Graphics;
using Input;
using UI;

internal readonly record struct TextBoxStateData
{
    private readonly int charIndex;

    public StringBuilder Text { get; init; }

    public int TextLength { get; init; }

    public int TextLeft { get; init; }

    public int TextRight { get; init; }

    public RectShape TextView { get; init; }

    public KeyCode Key { get; init; }

    /// <summary>
    /// Gets the index of the cursor.
    /// </summary>
    public int CharIndex
    {
        readonly get => this.charIndex;
        init => this.charIndex = value < 0 ? 0 : value;
    }

    public int CurrentCharLeft { get; init; }

    public int CurrentCharRight { get; init; }

    public bool InSelectionMode { get; init; }

    public RectangleF SelStartCharBounds { get; init; }

    public RectangleF SelStopCharBounds { get; init; }

    public RectangleF LastVisibleCharBounds { get; init; }

    public RectangleF FirstVisibleCharBounds { get; init; }

    public int SelectionStartIndex { get; init; }

    public int SelectionStopIndex { get; init; }

    public int SelectionHeight { get; init; }

    public bool SelectionAtRightEnd { get; init; }

    public Vector2 Position { get; init; }

    public bool CursorAtEnd { get; init; }

    public MutateType TextMutateType { get; init; }

    public TextBoxEvent Event { get; init; }

    public int Width { get; init; }
}
