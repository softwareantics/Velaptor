// <copyright file="TextCursor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using System;
using System.Collections.Generic;
using System.Drawing;
using Graphics;
using Input;

internal class TextCursor : ITextCursor
{
    private readonly List<(char character, RectangleF bounds)> characterBounds;
    private RectShape cursor;
    private bool charIndexWasAtStart;
    private bool charIndexWasAtEnd;
    private bool cursorWasAtRightEndOfText;
    private bool cursorWasNotAtRightEndOfText;

    public TextCursor(List<(char character, RectangleF bounds)> characterBounds) =>
        this.characterBounds = characterBounds;

    public RectShape Cursor
    {
        get => this.cursor;
        set => this.cursor = value;
    }

    public TextBoxState TextBoxState { get; set; }

    public void PreTextMutate()
    {
        this.charIndexWasAtStart = TextBoxState.CharIndex == 0;
        this.charIndexWasAtEnd = TextBoxState.CharIndex >= TextBoxState.TextLength - 1;
        this.cursorWasAtRightEndOfText = this.characterBounds.Count <= 0 || Cursor.Right > this.characterBounds.TextRight();
        this.cursorWasNotAtRightEndOfText = !this.cursorWasAtRightEndOfText;
    }

    public void PostTextMutate()
    {
    }

    public TextBoxState UpdateCursorState(TextBoxState state) => TextBoxState = state;

    public void AdjustCursor(KeyCode key, TextBoxEvent textBoxEvent)
    {
        // TODO: Move some of these state vars right above the correct switch statements if applicable
        var charIndexIsAtStart = TextBoxState.CharIndex <= 0;
        var charIndexIsAtEnd = TextBoxState.CharIndex >= TextBoxState.TextLength - 1;
        var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;
        var charIndexIsAtStartOrMiddle = charIndexIsAtStart || charIndexIsInMiddle;
        var cursorIsAtRightEndOfText = TextBoxState.TextLength <= 0 || this.cursor.Right > this.characterBounds.TextRight();
        var cursorIsNotAtRightEndOfText = !cursorIsAtRightEndOfText;

        switch (textBoxEvent)
        {
            case TextBoxEvent.AddingCharacter:
                AddingCharEvent();
                break;
            case TextBoxEvent.RemovingSingleChar:
                RemoveSingleCharEvent(key);
                break;
            case TextBoxEvent.RemovingSelectedChars:
                if (charIndexIsAtStartOrMiddle)
                {
                    SetCursorToCharLeft();
                }
                else if (charIndexIsAtEnd && cursorIsAtRightEndOfText)
                {
                    SetCursorToCharRight();
                }
                else if (charIndexIsAtEnd && cursorIsNotAtRightEndOfText)
                {
                    SetCursorToCharLeft();
                }

                break;
            case TextBoxEvent.MovingCursor:
                MovingCursorEvent(key);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(textBoxEvent), textBoxEvent, null);
        }

        if (Cursor.Left < TextBoxState.Left)
        {
            Cursor = Cursor with { Left = TextBoxState.Left, };
        }

        if (Cursor.Left > TextBoxState.Right)
        {
            Cursor = Cursor with { Left = TextBoxState.Right, };
        }
    }

    private void MovingCursorEvent(KeyCode key)
    {
        var charIndexIsAtStart = TextBoxState.CharIndex <= 0;
        var charIndexIsAtEnd = TextBoxState.CharIndex >= TextBoxState.TextLength - 1;
        var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;

        var charIndexWasNotAtStart = !this.charIndexWasAtStart;
        var charIndexWasNotAtEnd = !this.charIndexWasAtEnd;

        switch (key)
        {
            case KeyCode.Left when charIndexIsAtStart:
                MoveCursorToTextLeft();
                break;
            case KeyCode.Left when this.charIndexWasAtEnd:
                SetCursorToCharLeft();
                break;
            case KeyCode.Left when charIndexIsInMiddle:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when this.charIndexWasAtStart && TextBoxState.TextLength > 1:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when charIndexWasNotAtStart && charIndexIsInMiddle:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when charIndexWasNotAtEnd && charIndexIsAtEnd:
                SetCursorToCharLeft();
                break;
            case KeyCode.Right when this.cursorWasNotAtRightEndOfText && charIndexIsAtEnd:
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
        var charIndexIsAtStart = TextBoxState.CharIndex <= 0;
        var charIndexIsAtEnd = TextBoxState.CharIndex >= TextBoxState.TextLength - 1;
        var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;

        switch (TextBoxState.TextLargerThanView)
        {
            case true when charIndexIsInMiddle:
                SetCursorToCharLeft();
                break;
            case true when charIndexIsAtEnd && this.cursorWasAtRightEndOfText:
                SetCursorToCharRight();
                break;
            case false when charIndexIsAtEnd:
                if (this.cursorWasAtRightEndOfText)
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
        var charIndexIsAtStart = TextBoxState.CharIndex <= 0;
        var charIndexIsAtEnd = TextBoxState.CharIndex >= TextBoxState.TextLength - 1;
        var charIndexIsInMiddle = charIndexIsAtStart is false && charIndexIsAtEnd is false;
        var charIndexIsStartOrMiddle = charIndexIsAtStart || charIndexIsInMiddle;
        var cursorIsAtRightEndOfText = TextBoxState.TextLength <= 0 || this.cursor.Right > this.characterBounds.TextRight();
        var cursorIsNotAtRightEndOfText = !cursorIsAtRightEndOfText;

        var charIndexWasNotAtEnd = !this.charIndexWasAtEnd;

        switch (key)
        {
            case KeyCode.Backspace when TextBoxState.TextLength <= 0:
                MoveCursorToTextLeft();
                break;
            case KeyCode.Backspace when this.charIndexWasAtEnd && this.cursorWasNotAtRightEndOfText && charIndexIsAtEnd:
            case KeyCode.Backspace when charIndexIsStartOrMiddle && cursorIsNotAtRightEndOfText:
                SetCursorToCharLeft();
                break;
            case KeyCode.Delete when charIndexIsInMiddle || (charIndexWasNotAtEnd && charIndexIsAtEnd):
            // case KeyCode.Delete when charIndexWasNotAtEnd && charIndexIsAtEnd:
                SetCursorToCharLeft();
                break;
            case KeyCode.Backspace or KeyCode.Delete when this.charIndexWasAtEnd && this.cursorWasAtRightEndOfText && charIndexIsAtEnd:
                SetCursorToCharRight();
                break;
            case KeyCode.Delete when charIndexIsAtEnd:
                SetCursorToCharRight();
                break;
        }
    }

    private RectangleF GetCurrentCharBounds() =>
        this.characterBounds.Count <= 0
            ? default
            : TextBoxState.CurrentCharBounds;

    private void SetCursorToCharLeft() =>
        this.cursor.Left = TextBoxState.TextLength <= 0
            ? TextBoxState.Left
            : GetCurrentCharBounds().Left;

    private void SetCursorToCharRight() =>
        this.cursor.Left = TextBoxState.TextLength <= 0
            ? TextBoxState.Left
            : GetCurrentCharBounds().Right;

    private void MoveCursorToTextLeft()
    {
        if (this.characterBounds.Count <= 0)
        {
            this.cursor.Left = TextBoxState.Left;
            return;
        }

        this.cursor.Left = this.characterBounds[0].bounds.Left;
    }

    private void MoveCursorToTextRight()
    {
        if (this.characterBounds.Count <= 0)
        {
            return;
        }

        this.cursor.Left = this.characterBounds.TextRight();
    }
}
