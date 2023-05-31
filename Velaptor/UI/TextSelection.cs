namespace Velaptor.UI;

using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Graphics;
using Input;

public class TextSelection : ITextSelection
{
    private int selectionStartIndex;
    private int selectionStopIndex;
    private bool cursorStartedAtEnd;
    private bool cursorIsAtEnd;
    private bool inSelectionMode;
    private bool prevInSelectionMode;

    public bool InSelectionMode
    {
        get => this.inSelectionMode;
        set
        {
            this.prevInSelectionMode = this.inSelectionMode;
            this.inSelectionMode = value;
        }
    }

    public RectShape SelectionRect { get; private set; }

    public Vector2 Position { get; set; }

    public Color SelectionColor
    {
        get => SelectionRect.Color;
        set => SelectionRect = SelectionRect with { Color = value };
    }

    public int SelectionStartIndex
    {
        get => this.selectionStartIndex;
        set
        {
            this.selectionStartIndex = value < 0 ? 0 : value;

            if (this.selectionStartIndex == SelectionStopIndex)
            {
                SelectedText = string.Empty;
            }
        }
    }

    // TODO: Create backing field and limit to 0 or more
    public int SelectionStopIndex
    {
        get => this.selectionStopIndex;
        set
        {
            this.selectionStopIndex = value < 0 ? 0 : value;

            if (this.selectionStartIndex == SelectionStopIndex)
            {
                SelectedText = string.Empty;
            }
        }
    }

    public string SelectedText { get; private set; }

    public int Height { get; set; }

    public RectangleF TextViewArea { get; set; }

    public void Clear()
    {
        this.inSelectionMode = false;
        this.selectionStartIndex = 0;
        this.selectionStopIndex = 0;
        SelectedText = string.Empty;
    }

    // TODO: Convert the charBounds to the list of tuples just like in the TextBox
    public void UpdateSelectionRect(
        KeyCode key,
        RectangleF[] charBounds,
        RectShape cursorBounds,
        StringBuilder text)
    {
        var isMoveCursorKey = key is KeyCode.Left or KeyCode.Right or
                                 KeyCode.PageUp or KeyCode.PageDown or
                                 KeyCode.Home or KeyCode.End;
        var isNotMoveCursorKey = !isMoveCursorKey;

        var isNotVisibleKey = !key.IsVisibleKey();
        var isNotDeletionKey = key is not KeyCode.Delete && key is not KeyCode.Backspace;

        if (isNotMoveCursorKey && isNotVisibleKey && isNotDeletionKey)
        {
            return;
        }

        var startIndex = Math.Min(this.selectionStartIndex, this.selectionStopIndex);
        var endIndex = Math.Max(this.selectionStartIndex, this.selectionStopIndex);

        // Limit range of the end value
        var max = text.IsEmpty() ? 0 : text.Length - 1;
        endIndex = Math.Clamp(endIndex, 0, max);

        var prevCursorAtEnd = this.cursorIsAtEnd;
        this.cursorIsAtEnd = charBounds.IsEmpty() || cursorBounds.Right > charBounds[^1].Right;

        if (!this.inSelectionMode)
        {
            return;
        }

        if (this.prevInSelectionMode is false && this.inSelectionMode)
        {
            this.cursorStartedAtEnd = prevCursorAtEnd;
        }

        var indexDelta = Math.Abs(startIndex - endIndex);
        int selectionLen;

        var rectLeft = charBounds.IsEmpty() ? 0 : charBounds[startIndex].Left;
        float rectRight;

        var movingToTheRight = key is KeyCode.Right or KeyCode.End or KeyCode.PageDown;

        if (movingToTheRight)
        {
            if (this.cursorStartedAtEnd)
            {
                selectionLen = this.cursorIsAtEnd ? indexDelta : indexDelta + 1;
                rectRight = this.cursorIsAtEnd ? charBounds[endIndex].Left : charBounds[endIndex].Right;

                // Go out of selection mode if the cursor started at the end
                // and if the cursor is moving to the right
                this.inSelectionMode = !this.cursorIsAtEnd;
            }
            else
            {
                selectionLen = this.cursorIsAtEnd
                    ? indexDelta + 1
                    : indexDelta;

                rectRight = this.cursorIsAtEnd
                    ? charBounds[endIndex].Right
                    : charBounds[endIndex].Left;
            }
        }
        else
        {
            selectionLen = this.cursorStartedAtEnd
                ? indexDelta + 1
                : indexDelta;

            if (charBounds.IsEmpty())
            {
                rectRight = 0;
            }
            else
            {
                rectRight = this.cursorStartedAtEnd
                    ? charBounds[endIndex].Right
                    : charBounds[endIndex].Left;
            }
        }

        if (this.inSelectionMode is false)
        {
            return;
        }

        var cursorDidNotStartAtEnd = !this.cursorStartedAtEnd;
        var cursorIsNotAtEnd = !this.cursorIsAtEnd;

        var somethingSelected = (cursorDidNotStartAtEnd && this.cursorIsAtEnd) ||
                                (this.cursorStartedAtEnd && cursorIsNotAtEnd) ||
                                startIndex != endIndex;

        SelectedText = somethingSelected
            ? text.Substring(startIndex, selectionLen)
            : string.Empty;

        var rectWidth = Math.Abs(rectRight - rectLeft);
        var smallestX = Math.Min(rectLeft, rectRight);

        SelectionRect = SelectionRect with
        {
            IsSolid = true,
            Position = new Vector2(SelectionRect.Position.X, Position.Y),
            Width = rectWidth,
            Height = Height,
            Left = smallestX,
        };
    }
}
