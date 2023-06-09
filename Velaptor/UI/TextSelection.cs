// <copyright file="TextSelection.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using System;
using System.Drawing;
using System.Numerics;
using Carbonate.UniDirectional;
using Graphics;
using Input;
using ReactableData;

internal class TextSelection : ITextSelection
{
    // TODO: Need to figure out unsubscription
    private readonly Guid textBoxDataEventId = new ("71931561-826B-431B-BCE6-B139034A1FF4");
    private readonly IDisposable textBoxDataUnsubscriber;
    private readonly IDisposable textCursorDataUnsubscriber;
    private TextBoxStateData preMutateState;
    private TextBoxStateData postMutateState;
    private bool prevInSelectionMode;
    private bool cursorIsAtEnd;
    private bool cursorWasAtEnd;

    public TextSelection(IPushReactable<TextBoxStateData> textBoxDataReactable)
    {
        this.textBoxDataUnsubscriber = textBoxDataReactable.Subscribe(new ReceiveReactor<TextBoxStateData>(
            eventId: this.textBoxDataEventId,
            name: "TextBoxStateDataUpdate",
            onReceiveData: textBoxData =>
            {
                switch (textBoxData.TextMutateType)
                {
                    case MutateType.PreMutate:
                        this.preMutateState = textBoxData;
                        break;
                    case MutateType.PostMutate:
                        this.postMutateState = textBoxData;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }));
    }

    public RectShape SelectionRect { get; private set; }

    public string SelectedText { get; private set; }

    public Color SelectionColor
    {
        get => SelectionRect.Color;
        set => SelectionRect = SelectionRect with { Color = value };
    }

    // TODO: Convert the charBounds to the list of tuples just like in the TextBox
    public void Update()
    {
        var key = this.postMutateState.Key;
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

        var startIndex = Math.Min(this.postMutateState.SelectionStartIndex, this.postMutateState.SelectionStopIndex);
        var endIndex = Math.Max(this.postMutateState.SelectionStartIndex, this.postMutateState.SelectionStopIndex);

        // Limit range of the end value
        var max = this.postMutateState.Text.IsEmpty() ? 0 : this.postMutateState.Text.Length - 1;
        endIndex = Math.Clamp(endIndex, 0, max);

        var movingToTheRight = key is KeyCode.Right or KeyCode.End or KeyCode.PageDown;
        var charIndexIsAtStart = this.postMutateState.CharIndex <= 0;
        var charIndexIsAtEnd = this.postMutateState.CharIndex >= this.postMutateState.TextLength - 1;
        var charIndexIsNotAtEnd = !charIndexIsAtEnd;
        var charIndexWasAtEnd = this.preMutateState.CharIndex >= this.preMutateState.TextLength - 1;
        var charIndexWasNotAtEnd = !charIndexWasAtEnd;
        var selectionCharIndexStartedAtEnd = this.preMutateState.SelectionStartIndex >= this.preMutateState.TextLength - 1;
        var selectionStartInMiddle = this.postMutateState.SelectionStartIndex < this.postMutateState.TextLength - 1;
        var cursorIsNotAtEnd = !this.cursorIsAtEnd;
        var cursorWasNotAtEnd = !this.cursorWasAtEnd;

        // TODO: Look into maybe removing this if block
        if (!this.postMutateState.InSelectionMode)
        {
            this.cursorWasAtEnd = this.cursorIsAtEnd;
            return;
        }

        // if (this.preMutateState.InSelectionMode is false && this.postMutateState.InSelectionMode)
        // {
        //     this.cursorStartedAtEnd = prevCursorAtEnd;
        // }

        // TODO: To be removed
        var indexDelta = Math.Abs(startIndex - endIndex);
        int selectionLen;

        var rectLeft = this.postMutateState.Text.IsEmpty()
            ? 0
            : Math.Min(this.postMutateState.SelStartCharBounds.Left, this.postMutateState.SelStopCharBounds.Left);
        var rectRight = 0f;

        if (movingToTheRight)
        {
            if (this.postMutateState.SelectionStartedAtRightEnd)
            {
                selectionLen = indexDelta + 1;
                rectRight = Math.Max(this.postMutateState.SelStartCharBounds.Right, this.postMutateState.SelStopCharBounds.Right);
            }
            else if (charIndexIsAtEnd && charIndexWasAtEnd)
            {
                selectionLen = indexDelta + 1;
                rectRight = Math.Max(this.postMutateState.SelStartCharBounds.Right, this.postMutateState.SelStopCharBounds.Right);
            }
            else if (key is KeyCode.PageDown or KeyCode.End || (selectionStartInMiddle && cursorWasNotAtEnd && charIndexWasAtEnd))
            {
                selectionLen = indexDelta + 1;
                rectRight = Math.Max(this.postMutateState.SelStartCharBounds.Right, this.postMutateState.SelStopCharBounds.Right);
            }
            else
            {
                selectionLen = indexDelta;
                rectRight = Math.Max(this.postMutateState.SelStartCharBounds.Left, this.postMutateState.SelStopCharBounds.Left);
            }
        }
        else
        {
            if (this.postMutateState.SelectionStartedAtRightEnd)
            {
                selectionLen = indexDelta + 1;
                rectRight = this.postMutateState.SelStartCharBounds.Right;
            }
            else if (charIndexWasAtEnd && charIndexIsNotAtEnd)
            {
                selectionLen = indexDelta;
                rectRight = Math.Max(this.postMutateState.SelStartCharBounds.Left, this.postMutateState.SelStopCharBounds.Left);
            }
            else
            {
                selectionLen = indexDelta;
                rectRight = Math.Max(this.postMutateState.SelStartCharBounds.Left, this.postMutateState.SelStopCharBounds.Left);
            }
        }

        if (this.postMutateState.InSelectionMode is false)
        {
            return;
        }

        rectLeft = rectLeft < this.postMutateState.TextViewLeft
            ? this.postMutateState.TextViewLeft
            : rectLeft;

        rectRight = rectRight > this.postMutateState.TextViewRight
            ? this.postMutateState.TextViewRight
            : rectRight;

        SelectedText = selectionLen > 0
            ? this.postMutateState.Text.Substring(startIndex, selectionLen)
            : string.Empty;

        var rectWidth = Math.Abs(rectRight - rectLeft);
        var smallestX = Math.Min(rectLeft, rectRight);

        SelectionRect = SelectionRect with
        {
            IsSolid = true,
            Position = new Vector2(SelectionRect.Position.X, this.postMutateState.Position.Y),
            Width = rectWidth,
            Height = this.postMutateState.SelectionHeight,
            Left = smallestX,
        };

        this.cursorWasAtEnd = this.cursorIsAtEnd;
    }

    public void Clear()
    {
        this.preMutateState = default;
        this.postMutateState = default;
        SelectedText = string.Empty;
    }
}
