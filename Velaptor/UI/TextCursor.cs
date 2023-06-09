// <copyright file="TextCursor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using System;
using Carbonate.UniDirectional;
using Graphics;
using Input;
using ReactableData;

internal class TextCursor : ITextCursor
{
    // TODO: Need to figure out unsubscription
    private readonly Guid textBoxDataEventId = new ("71931561-826B-431B-BCE6-B139034A1FF4");
    private readonly IDisposable unsubscriber;
    private TextBoxStateData preMutateState;
    private TextBoxStateData postMutateState;
    private RectShape prevCursor;
    private RectShape currentCursor;

    // private bool charIndexWasAtStart;
    // private bool charIndexWasAtEnd;
    // private bool cursorWasAtRightEndOfText;
    // private bool cursorWasNotAtRightEndOfText;

    public TextCursor(IPushReactable<TextBoxStateData> textBoxStateReactable)
    {
        this.unsubscriber = textBoxStateReactable.Subscribe(new ReceiveReactor<TextBoxStateData>(
            eventId: this.textBoxDataEventId,
            name: "TextBoxStateDataUpdate",
            onReceiveData: data =>
            {
                switch (data.TextMutateType)
                {
                    case MutateType.PreMutate:
                        this.preMutateState = data;
                        break;
                    case MutateType.PostMutate:
                        this.postMutateState = data;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }));
    }

    public RectShape Cursor
    {
        get => this.currentCursor;
        set => this.currentCursor = value;
    }

    public TextBoxState TextBoxState { get; set; }

    public void Update()
    {
        switch (this.postMutateState.Event)
        {
            case TextBoxEvent.None:
                break;
            case TextBoxEvent.AddingCharacter:
                AddingCharEvent();
                break;
            case TextBoxEvent.RemovingSingleChar:
                RemoveSingleCharEvent(this.postMutateState.Key);
                break;
            case TextBoxEvent.RemovingSelectedChars:
                var charIndexIsAtStart = this.postMutateState.CharIndex <= 0;
                var charIndexWasAtEnd = this.preMutateState.CharIndex >= this.preMutateState.TextLength - 1;
                var charIndexIsAtEnd = this.postMutateState.CharIndex >= this.postMutateState.TextLength - 1;
                var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;
                var charIndexIsAtStartOrMiddle = charIndexIsAtStart || charIndexIsInMiddle;
                var cursorWasAtRightEndOfText = this.preMutateState.CursorAtEnd;
                var cursorWasNotAtRightEndOfText = !cursorWasAtRightEndOfText;
                var cursorIsAtRightEndOfText = this.postMutateState.CursorAtEnd;
                var cursorIsNotAtRightEndOfText = !cursorIsAtRightEndOfText;
                var selectionRightToLeft = this.postMutateState.SelectionStartIndex > this.postMutateState.SelectionStopIndex;
                var selectionLeftToRight = this.postMutateState.SelectionStopIndex > this.postMutateState.SelectionStartIndex;

                if (charIndexIsAtStartOrMiddle && this.postMutateState.TextLength > 1)
                {
                    SetCursorToCharLeft();
                }
                else if (charIndexWasAtEnd && charIndexIsAtEnd && selectionRightToLeft)
                {
                    SetCursorToCharRight();
                }
                else if (charIndexWasAtEnd && charIndexIsAtEnd && selectionLeftToRight)
                {
                    SetCursorToCharRight();
                }
                else if (charIndexIsAtEnd && cursorWasNotAtRightEndOfText && cursorIsAtRightEndOfText)
                {
                    SetCursorToCharLeft();
                }
                else if (charIndexIsAtEnd && cursorIsNotAtRightEndOfText)
                {
                    SetCursorToCharLeft();
                }

                break;
            case TextBoxEvent.MovingCursor:
                MovingCursorEvent(this.postMutateState.Key);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(this.postMutateState.Event), this.postMutateState.Event, null);
        }

        if (this.currentCursor.Left < this.postMutateState.TextLeft)
        {
            this.currentCursor = this.currentCursor with { Left = this.postMutateState.TextLeft, };
        }

        if (this.currentCursor.Left > this.postMutateState.TextRight)
        {
            this.currentCursor = this.currentCursor with { Left = this.postMutateState.TextRight, };
        }
    }

    private void MovingCursorEvent(KeyCode key)
    {
        var charIndexWasAtStart = this.preMutateState.CharIndex == 0;
        var charIndexWasAtEnd = this.preMutateState.CharIndex >= this.preMutateState.TextLength - 1;
        var cursorWasAtRightEndOfText = this.preMutateState.TextLength <= 0 || Cursor.Right > this.preMutateState.TextRight;
        var cursorWasNotAtRightEndOfText = !cursorWasAtRightEndOfText;

        var charIndexIsAtStart = this.postMutateState.CharIndex <= 0;
        var charIndexIsAtEnd = this.postMutateState.CharIndex >= this.postMutateState.TextLength - 1;
        var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;

        var charIndexWasNotAtStart = !charIndexWasAtStart;
        var charIndexWasNotAtEnd = !charIndexWasAtEnd;

        switch (key)
        {
            case KeyCode.Left when charIndexIsAtStart:
                MoveCursorToTextLeft();
                break;
            case KeyCode.Left when charIndexWasAtEnd:
                SetCursorToCharLeft();
                break;
            case KeyCode.Left when charIndexIsInMiddle:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when charIndexWasAtStart && this.postMutateState.TextLength > 1:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when charIndexWasNotAtStart && charIndexIsInMiddle:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when charIndexWasNotAtEnd && charIndexIsAtEnd:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when cursorWasNotAtRightEndOfText && charIndexIsAtEnd:
                SetCursorToCharRight();
                break;
            case KeyCode.Home:
            case KeyCode.PageUp:
                MoveCursorToTextLeft();
                break;
            case KeyCode.End:
            case KeyCode.PageDown:
                MoveCursorToTextRight();
                break;
        }
    }

    private void AddingCharEvent()
    {
        var cursorWasAtRightEndOfText = this.preMutateState.TextLength <= 0 || Cursor.Right > this.preMutateState.TextRight;

        var charIndexIsAtStart = this.postMutateState.CharIndex <= 0;
        var charIndexIsAtEnd = this.postMutateState.CharIndex >= this.postMutateState.TextLength - 1;
        var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;
        var textLargerThanView = this.postMutateState.IsTextLargerThanView();

        switch (textLargerThanView)
        {
            case true when charIndexIsInMiddle:
                SetCursorToCharLeft();
                break;
            case true when charIndexIsAtEnd && cursorWasAtRightEndOfText:
                SetCursorToCharRight();
                break;
            case false when charIndexIsAtEnd:
                if (cursorWasAtRightEndOfText)
                {
                    SetCursorToCharRight();
                }
                else
                {
                    SetCursorToCharLeft();
                }

                break;
            case false when charIndexIsInMiddle:
                SetCursorToCharLeft();
                break;
        }
    }

    private void RemoveSingleCharEvent(KeyCode key)
    {
        // NOTE: Do not use the 'Text' property to get its length on the preMutateState.  It is a reference
        // to the original text and does not reflect the state before the text was mutated.
        var charIndexWasAtStart = this.preMutateState.CharIndex <= 0;
        var charIndexWasNotAtStart = !charIndexWasAtStart;
        var charIndexWasAtEnd = this.preMutateState.CharIndex >= this.preMutateState.TextLength - 1;
        var charIndexWasNotAtEnd = !charIndexWasAtEnd;
        var charIndexWasInMiddle = charIndexWasNotAtStart && charIndexWasNotAtEnd;
        var cursorWasAtRightEndOfText = this.preMutateState.TextLength <= 0 || Cursor.Right > this.preMutateState.TextRight;
        var cursorWasNotAtRightEndOfText = !cursorWasAtRightEndOfText;

        var charIndexIsAtStart = this.postMutateState.CharIndex <= 0;
        var charIndexIsAtEnd = this.postMutateState.CharIndex >= this.postMutateState.Text.Length - 1;
        var charIndexIsNotAtEnd = !charIndexIsAtEnd;
        var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;

        var charIndexIsStartOrMiddle = charIndexIsAtStart || charIndexIsInMiddle;
        var cursorIsAtRightEndOfText = this.postMutateState.Text.IsEmpty() || this.currentCursor.Right > this.postMutateState.TextRight;
        var cursorIsNotAtRightEndOfText = !cursorIsAtRightEndOfText;

        switch (key)
        {
            case KeyCode.Backspace when this.postMutateState.Text.IsEmpty():
                MoveCursorToTextLeft();
                break;
            case KeyCode.Backspace when charIndexWasAtEnd && cursorWasNotAtRightEndOfText && charIndexIsAtEnd:
            case KeyCode.Backspace when charIndexIsStartOrMiddle && cursorIsNotAtRightEndOfText:
                SetCursorToCharLeft();
                break;
            case KeyCode.Delete when charIndexWasAtEnd && cursorWasNotAtRightEndOfText && charIndexIsAtEnd && cursorIsAtRightEndOfText:
                SetCursorToCharRight(); // GOOD
                break;
            case KeyCode.Delete when charIndexWasInMiddle:
                SetCursorToCharLeft();
                break;
            case KeyCode.Delete when charIndexIsAtStart && charIndexIsAtEnd && cursorWasNotAtRightEndOfText:
                SetCursorToCharLeft();
                break;
            case KeyCode.Delete when charIndexIsAtEnd && cursorWasNotAtRightEndOfText && cursorIsNotAtRightEndOfText:
                SetCursorToCharRight();
                break;
            case KeyCode.Delete when charIndexIsNotAtEnd && cursorWasNotAtRightEndOfText && cursorIsNotAtRightEndOfText:
                break;
            case KeyCode.Backspace or KeyCode.Delete when charIndexWasAtEnd && cursorWasAtRightEndOfText && charIndexIsAtEnd:
                SetCursorToCharRight();
                break;
            // case KeyCode.Delete when charIndexIsAtEnd:
            //     SetCursorToCharRight();
            //     break;
        }
    }

    private void SetCursorToCharLeft() =>
        this.currentCursor.Left = this.postMutateState.Text.IsEmpty()
            ? this.postMutateState.TextLeft
            : this.postMutateState.CurrentCharLeft;

    private void SetCursorToCharRight() =>
        this.currentCursor.Left = this.postMutateState.Text.IsEmpty()
            ? this.postMutateState.TextLeft
            : this.postMutateState.CurrentCharRight;

    private void MoveCursorToTextLeft()
    {
        if (this.postMutateState.Text.IsEmpty())
        {
            this.currentCursor.Left = this.postMutateState.TextLeft;
            return;
        }

        this.currentCursor.Left = this.postMutateState.TextLeft;
    }

    private void MoveCursorToTextRight()
    {
        if (this.postMutateState.Text.IsEmpty())
        {
            return;
        }

        this.currentCursor.Left = this.postMutateState.TextRight;
    }
}
