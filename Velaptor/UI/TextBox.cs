// <copyright file="TextBox.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Content;
using Content.Fonts;
using Factories;
using Graphics;
using Graphics.Renderers;
using Input;
using Color = System.Drawing.Color;

/// <summary>
/// Provides the ability to enter text into a box.
/// </summary>
public sealed class TextBox : ControlBase
{
    private const uint CursorBlinkRate = 500;
    private const int MarginTop = 12;
    private const int MarginLeft = 10; // TODO:  Change these back to a value of 2
    private const int MarginRight = 10; // TODO:  Change these back to a value of 2
    private const int MaxCtrlHeight = 42;
    private const int BorderThickness = 2;
    private readonly IShapeRenderer shapeRenderer;
    private readonly IFontRenderer fontRenderer;
    private readonly ITextSelection textSelection;
    private readonly ITextCursor textCursor;
    private readonly StringBuilder text = new ();
    private readonly List<(char character, RectangleF bounds)> charBounds = new ();
    private readonly bool renderAllText = false;
    private IFont font;
    private Vector2 textPos;
    private RectShape ctrlBorder;
    private RectShape textAreaBounds;
    private KeyboardState currKeyState;
    private SizeF textSize;
    private uint height = MaxCtrlHeight;
    private float cursorBlinkTimeElapsed;
    private bool cursorVisible;
    private bool prevInSelectionMode;
    private Color fontColor = Color.White;
    private float maxWidth;
    private uint width = 100;

    // TODO: Create internal ctor to inject everything for testing
    // TODO: The TextBoxState in the TextCursor is being used for holding state for logic
    // in the TextBox ctrl class.  This is not ideal. Refactor the code to separate these

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBox"/> class.
    /// </summary>
    public TextBox()
    {
        // TODO: TextCursor needs to be created via the IoC container
        this.textCursor = new TextCursor(this.charBounds);

        // TODO: TextSelection needs to be created via the IoC container
        this.textSelection = new TextSelection
        {
            SelectionColor = Color.CornflowerBlue,
        };

        var rendererFactory = new RendererFactory();
        this.shapeRenderer = rendererFactory.CreateShapeRenderer();
        this.fontRenderer = rendererFactory.CreateFontRenderer();
        Keyboard = IoC.Container.GetInstance<IAppInput<KeyboardState>>();
    }

    internal TextBox(IRendererFactory rendererFactory, IAppInput<KeyboardState> keyboard)
    {
        this.shapeRenderer = rendererFactory.CreateShapeRenderer();
        this.fontRenderer = rendererFactory.CreateFontRenderer();

        this.textCursor = new TextCursor(this.charBounds);
        this.textSelection = new TextSelection
        {
            SelectionColor = Color.CornflowerBlue,
        };

        Keyboard = keyboard;
    }

    /// <summary>
    /// Gets or sets the text in the <see cref="TextBox"/>.
    /// </summary>
    public string Text
    {
        get => this.text.ToString();
        set
        {
            this.text.Clear();
            this.text.Append(value);

            this.textCursor.TextBoxState = this.textCursor.TextBoxState with
            {
                CharIndex = Math.Clamp(value.Length, 0, this.textCursor.TextBoxState.CharIndex),
            };
        }
    }

    public string SelectedText => this.textSelection.SelectedText;

    public Color TextColor
    {
        get => this.fontColor;
        set
        {
            this.fontColor = value;
            this.textSelection.SelectionColor = value.Inverse();
        }
    }

    public override uint Width
    {
        get => this.width;
        set => this.width = value;
    }

    public override uint Height
    {
        get => this.height;
        set => this.height = value < MaxCtrlHeight ? MaxCtrlHeight : value;
    }

    /// <summary>
    /// Loads the content of the <see cref="TextBox"/>.
    /// </summary>
    public override void LoadContent()
    {
        var contentLoader = ContentLoaderFactory.CreateContentLoader();
        this.font = contentLoader.LoadFont("TimesNewRoman-Regular", 12);

        this.maxWidth = this.font.Metrics.Max(m => m.GlyphWidth);

        UpdateBorder();

        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            Left = Left + MarginLeft,
            Right = Right - MarginRight,
        };

        var areaBoundsWidth = this.ctrlBorder.Right - this.ctrlBorder.Left;
        areaBoundsWidth -= MarginLeft + MarginRight + (BorderThickness * 2);

        this.textAreaBounds = new RectShape
        {
            Position = Position.ToVector2(),
            Width = areaBoundsWidth,
            Height = Height,
            Color = Color.Orange,
            IsSolid = false,
        };

        this.textCursor.Cursor = new RectShape
        {
            Position = new Vector2(this.textAreaBounds.Left, Position.Y),
            Color = Color.CornflowerBlue,
            Width = 2,
            Height = Height - MarginTop,
            IsSolid = true,
        };

        this.textPos = new Vector2(this.textAreaBounds.Left, Position.Y);

        this.textSelection.Position = Position.ToVector2();
        this.textSelection.Height = (int)(Height - MarginTop);

        base.LoadContent();
    }

    /// <summary>
    /// Updates the text box.
    /// </summary>
    /// <param name="frameTime">The amount of time that has passed for the current frame.</param>
    public override void Update(FrameTime frameTime)
    {
        if (IsLoaded is false || Enabled is false)
        {
            return;
        }

        // Update the left and right ends of the text viewing area
        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            Left = Left + MarginLeft,
            Right = Right - MarginRight,
        };

        // TODO: Update this in the correct properties to avoid processing in update loop
        var textAreaX = Position.X - Width.Half() + (this.ctrlBorder.BorderThickness + MarginLeft);
        var textAreaY = Position.Y - Height.Half();
        var textAreaWidth = Width - (MarginLeft + MarginRight + this.ctrlBorder.BorderThickness);
        var textAreaHeight = Height;
        this.textSelection.TextViewArea = new RectangleF(textAreaX, textAreaY, textAreaWidth, textAreaHeight);

        this.cursorBlinkTimeElapsed += (float)frameTime.ElapsedTime.TotalMilliseconds;

        if (this.cursorBlinkTimeElapsed >= CursorBlinkRate)
        {
            this.cursorVisible = !this.cursorVisible;
            this.cursorBlinkTimeElapsed -= CursorBlinkRate;
        }

        this.cursorVisible = true;

        this.currKeyState = Keyboard.GetState();

        UpdateBorder();

        base.Update(frameTime);
    }

    /// <inheritdoc/>
    public override void Render()
    {
        if (IsLoaded is false || Visible is false)
        {
            return;
        }

        this.shapeRenderer.Render(this.ctrlBorder, layer: 0);

        if (this.cursorVisible)
        {
            this.shapeRenderer.Render(this.textCursor.Cursor, layer: 0);
        }

        // TODO: Improve this code
        // TODO: When selecting to the right and the sel rect is past the left end,
        // and we are unselecting text to the left, we cannot see any of the selection rect.
        if (this.textSelection.InSelectionMode)
        {
            var leftEndPastView = this.textSelection.SelectionRect.Left < this.textSelection.TextViewArea.Left;
            var rightEndPastView = this.textSelection.SelectionRect.Right > this.textSelection.TextViewArea.Right;

            var selectionRect = new RectShape();

            if (rightEndPastView && !leftEndPastView)
            {
                var amountPastEnd = this.textSelection.SelectionRect.Right - this.textSelection.TextViewArea.Right;

                selectionRect = this.textSelection.SelectionRect with
                {
                    Width = this.textSelection.SelectionRect.Width - amountPastEnd,
                    Color = Color.Orange, // debug
                };

                selectionRect.Left -= amountPastEnd.Half();
            }
            else if (leftEndPastView && !rightEndPastView)
            {
                var amountPastEnd = this.textSelection.TextViewArea.Left - this.textSelection.SelectionRect.Left;

                selectionRect = this.textSelection.SelectionRect with
                {
                    Width = this.textSelection.SelectionRect.Width - amountPastEnd,
                    Color = Color.SeaGreen, // debug
                };

                selectionRect.Left += amountPastEnd.Half();
            }
            else
            {
                selectionRect = this.textSelection.SelectionRect;
            }

            this.shapeRenderer.Render(selectionRect, layer: 10);
        }

        RenderText();
    }

    private void RenderText()
    {
        var glyphsToRender = this.charBounds.Select(cb =>
        {
            var inView = this.textAreaBounds.Contains(new Vector2(cb.bounds.Left, Position.Y)) &&
                             this.textAreaBounds.Contains(new Vector2(cb.bounds.Right, Position.Y));

            var glyphMetrics = this.font.Metrics.First(m => m.Glyph == cb.character);

            if (this.renderAllText || inView)
            {
                return glyphMetrics;
            }

            var tempBounds = glyphMetrics.GlyphBounds;
            tempBounds.Width = 0;
            glyphMetrics.GlyphBounds = tempBounds;

            return glyphMetrics;
        }).ToArray().AsSpan();

        this.fontRenderer.RenderBaseNEW(
            this.font,
            glyphsToRender,
            (int)this.textPos.X,
            (int)this.textPos.Y,
            1f,
            0f,
            TextColor,
            layer: 20);
    }

    protected override void OnKeyUp(KeyCode key)
    {
        var anyMoveCursorKeysDown = key.IsArrowKey() ||
                                    key == KeyCode.Home || key == KeyCode.End ||
                                    key == KeyCode.PageUp || key == KeyCode.PageDown;

        if (anyMoveCursorKeysDown)
        {
            if (this.currKeyState.AnyShiftKeysDown())
            {
                this.textSelection.InSelectionMode = true;
            }
            else
            {
                this.textSelection.InSelectionMode = false;
                // TODO; Set the indices to 0 when going to false in the InSelectionMode property
                this.textSelection.SelectionStartIndex = 0;
                this.textSelection.SelectionStopIndex = 0;
            }
        }

        if (this.textSelection.InSelectionMode && this.prevInSelectionMode is false)
        {
            this.textSelection.SelectionStartIndex = this.textCursor.TextBoxState.CharIndex;
        }

        this.textCursor.PreTextMutate();

        if (key.IsVisibleKey())
        {
            Add(key);
            this.textCursor.AdjustCursor(key, TextBoxEvent.AddingCharacter);
        }
        else if (KeyboardKeyGroups.DeletionKeys.Contains(key))
        {
            var cursorEvent = string.IsNullOrEmpty(this.textSelection.SelectedText) switch
            {
                true => TextBoxEvent.RemovingSingleChar,
                false => TextBoxEvent.RemovingSelectedChars,
            };

            Remove(key);

            this.textCursor.AdjustCursor(key, cursorEvent);
        }
        else if (KeyboardKeyGroups.CursorMovementKeys.Contains(key))
        {
            this.cursorVisible = true;

            SetCursorIndex(key);
            this.textCursor.AdjustCursor(key, TextBoxEvent.MovingCursor);
        }

        // Update the text rendering position
        var charBoundsCenter = this.charBounds.CenterPosition();
        this.textPos = this.textPos with { X = charBoundsCenter.X, };

        this.textSelection.UpdateSelectionRect(
            key,
            this.charBounds.Select(m => m.bounds).ToArray(),
            this.textCursor.Cursor,
            this.text);

        this.prevInSelectionMode = this.textSelection.InSelectionMode;
        base.OnKeyUp(key);
    }

    private void Add(KeyCode key)
    {
        var character = key == KeyCode.Space
            ? " "
            : ToChar(key).ToString();

        var textLength = this.textCursor.TextBoxState.TextLength;
        var charIndexAtEnd = this.textCursor.TextBoxState.CharIndex >= textLength - 1;
        var cursorAtRightEndOfText = this.text.IsEmpty() || this.textCursor.Cursor.Right > this.charBounds.TextRight();

        var insertIndex = charIndexAtEnd && cursorAtRightEndOfText
            ? textLength
            : this.textCursor.TextBoxState.CharIndex;

        var prevHeight = this.textSize.Height;

        this.text.Insert(insertIndex, character);
        UpdateTextBoxState();
        RebuildBounds();

        if (this.textCursor.TextBoxState.TextLargerThanView)
        {
            if (charIndexAtEnd)
            {
                SnapBoundsToRight();
            }
            else
            {
                var cursorWillBePastRightEnd =
                    this.charBounds[this.textCursor.TextBoxState.CharIndex].bounds.Right > this.textAreaBounds.Right;

                if (cursorWillBePastRightEnd)
                {
                    var overLap = this.charBounds[this.textCursor.TextBoxState.CharIndex].bounds.Right - this.textAreaBounds.Right;
                    this.charBounds.BumpAllToleft(overLap);
                }
            }
        }
        else
        {
            if (charIndexAtEnd && cursorAtRightEndOfText)
            {
                SnapBoundsToLeft();
            }
        }

        CalcYPositions(prevHeight, this.textSize.Height);

        var newCursorIndex = charIndexAtEnd
            ? this.textCursor.TextBoxState.TextLength - 1
            : this.textCursor.TextBoxState.CharIndex + 1;

        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            CharIndex = newCursorIndex,
            CurrentCharBounds = this.charBounds[newCursorIndex].bounds,
        };
    }

    private void Remove(KeyCode key)
    {
        if (this.text.IsEmpty())
        {
            return;
        }

        switch (key)
        {
            case KeyCode.Backspace when string.IsNullOrEmpty(this.textSelection.SelectedText):
                BackspaceSingleChar();
                break;
            case KeyCode.Delete when string.IsNullOrEmpty(this.textSelection.SelectedText):
                DeleteCurrentChar();
                break;
            case KeyCode.Backspace or KeyCode.Delete when !string.IsNullOrEmpty(this.textSelection.SelectedText):
                RemoveSelectedChars();
                break;
        }
    }

    private void BackspaceSingleChar()
    {
        var charIndexAtStart = this.textCursor.TextBoxState.CharIndex <= 0;
        var cursorAtRightEndOfText = this.textCursor.Cursor.Right > this.charBounds.TextRight();
        var cursorNotAtRightEndOfText = !cursorAtRightEndOfText;

        if (charIndexAtStart && cursorNotAtRightEndOfText)
        {
            return;
        }

        var prevHeight = this.textSize.Height;

        if (cursorAtRightEndOfText)
        {
            this.text.RemoveLastChar();
        }
        else
        {
            this.text.RemoveChar(this.textCursor.TextBoxState.CharIndex - 1);
        }

        UpdateTextBoxState();
        RebuildBounds();

        var isEmpty = this.text.IsEmpty();

        if (isEmpty)
        {
            this.charBounds.Clear();
        }

        var cursorIndex = cursorAtRightEndOfText
            ? Math.Clamp(this.textCursor.TextBoxState.TextLength - 1, 0, int.MaxValue)
            : this.textCursor.TextBoxState.CharIndex - 1;

        // Update the cursor index
        this.textCursor.TextBoxState = this.textCursor.TextBoxState with { CharIndex = cursorIndex, };
        var prevCharIndexAtStart = this.textCursor.TextBoxState.CharIndex - 1 <= 0;
        var prevCharIndexNotAtStart = !prevCharIndexAtStart;
        var textLargerThanView = this.textCursor.TextBoxState.TextLargerThanView;
        var gapAtRightEnd = GapAtRightEnd();
        var gapNotAtRightEnd = !gapAtRightEnd;

        switch (textLargerThanView)
        {
            case true when gapAtRightEnd:
                SnapBoundsToRight();
                break;
            case true when gapNotAtRightEnd && prevCharIndexNotAtStart:
                var bumpAmount = this.textAreaBounds.Left + MarginLeft - this.charBounds[cursorIndex - 1].bounds.Left;
                this.charBounds.BumpAllToRight(bumpAmount);

                if (GapAtRightEnd())
                {
                    SnapBoundsToRight();
                }

                break;
            case true when gapNotAtRightEnd && prevCharIndexAtStart:
            case false:
                SnapBoundsToLeft();
                break;
        }

        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            CurrentCharBounds = this.charBounds.IsEmpty()
                ? this.textCursor.TextBoxState.CurrentCharBounds
                : this.charBounds[cursorIndex].bounds,
        };

        CalcYPositions(prevHeight, this.textSize.Height);
    }

    private void DeleteCurrentChar()
    {
        var charIndexAtStart = this.textCursor.TextBoxState.CharIndex <= 0;
        var charIndexAtEnd = this.textCursor.TextBoxState.CharIndex >= this.textCursor.TextBoxState.TextLength - 1;
        var cursorAtRightEndOfText = this.textCursor.Cursor.Right > this.charBounds.TextRight();

        if (cursorAtRightEndOfText)
        {
            return;
        }

        var prevHeight = this.textSize.Height;

        this.text.RemoveChar(this.textCursor.TextBoxState.CharIndex);
        UpdateTextBoxState();
        RebuildBounds();

        var charIndexNotAtStart = !charIndexAtStart;

        if (this.textCursor.TextBoxState.TextLargerThanView)
        {
            var allRightEndTextVisible = charIndexAtEnd || (charIndexNotAtStart && GapAtRightEnd());
            if (allRightEndTextVisible)
            {
                SnapBoundsToRight();
            }
        }
        else
        {
            if (charIndexAtEnd || charIndexNotAtStart)
            {
                SnapBoundsToLeft();
            }
        }

        var cursorIndex = charIndexAtEnd
            ? this.textCursor.TextBoxState.CharIndex - 1
            : this.textCursor.TextBoxState.CharIndex;

        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            CharIndex = cursorIndex,
            CurrentCharBounds = this.text.IsEmpty()
                ? default
                : this.charBounds[cursorIndex].bounds,
        };

        CalcYPositions(prevHeight, this.textSize.Height);
    }

    private void RemoveSelectedChars()
    {
        var selectionToEndOfText = this.textSelection.SelectionRect.Right >= this.charBounds.TextRight();
        var startCharIndex = Math.Min(this.textSelection.SelectionStartIndex, this.textSelection.SelectionStopIndex);
        var stopCharIndex = selectionToEndOfText
            ? Math.Max(this.textSelection.SelectionStartIndex, this.textSelection.SelectionStopIndex) + 1
            : Math.Max(this.textSelection.SelectionStartIndex, this.textSelection.SelectionStopIndex);

        var removeLength = Math.Abs(startCharIndex - stopCharIndex);
        var prevHeight = this.textSize.Height;

        this.text.Remove(startCharIndex, removeLength);

        UpdateTextBoxState();
        RebuildBounds();
        this.textSelection.Clear();

        var textLargerThanView = this.textCursor.TextBoxState.TextLargerThanView;

        if (textLargerThanView)
        {
            if (GapAtRightEnd())
            {
                SnapBoundsToRight();
            }
        }
        else
        {
            SnapBoundsToLeft();
        }

        var cursorIndex = startCharIndex >= this.text.Length - 1
            ? this.text.Length - 1
            : startCharIndex;

        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            CharIndex = cursorIndex,
            CurrentCharBounds = this.text.IsEmpty()
                ? default
                : this.charBounds[cursorIndex].bounds,
        };

        CalcYPositions(prevHeight, this.textSize.Height);
    }

    private void MoveCharIndexLeft()
    {
        var charIndexAtStart = this.textCursor.TextBoxState.CharIndex <= 0;

        // If currently at the first char index
        if (charIndexAtStart)
        {
            SnapBoundsToLeft();
        }
        else
        {
            var currentCharLeft = this.charBounds.CharLeft(this.textCursor.TextBoxState.CharIndex);
            var anyTextLeftOfTextArea = currentCharLeft < this.textAreaBounds.Left;

            if (anyTextLeftOfTextArea)
            {
                var overlapAmount = this.textAreaBounds.Left - currentCharLeft;
                this.charBounds.BumpAllToRight(overlapAmount);
            }

            // If the cursor is at the beginning of the text
            if (this.textCursor.Cursor.Left <= this.charBounds[0].bounds.Left)
            {
                this.charBounds.BumpAllToRight(this.maxWidth);
            }
        }
    }

    private void MoveCharIndexRight()
    {
        if (!this.textCursor.TextBoxState.TextLargerThanView)
        {
            return;
        }

        var currentCharRight = this.charBounds.CharRight(this.textCursor.TextBoxState.CharIndex);
        var charIsNotPartiallyHidden = currentCharRight < this.textAreaBounds.Right;

        if (charIsNotPartiallyHidden)
        {
            return;
        }

        var overlapAmount = currentCharRight - this.textAreaBounds.Right;
        this.charBounds.BumpAllToleft(overlapAmount);

        if (GapAtRightEnd())
        {
            SnapBoundsToRight();
        }
    }

    private void SetCursorIndex(KeyCode key)
    {
        if (this.text.IsEmpty())
        {
            return;
        }

        var newIndex = CalcNextCharIndex(key);
        this.textCursor.TextBoxState = this.textCursor.TextBoxState with { CharIndex = newIndex, };
        this.textSelection.SelectionStopIndex = newIndex;

        switch (key)
        {
            case KeyCode.Left:
                MoveCharIndexLeft();
                break;
            case KeyCode.Right:
                MoveCharIndexRight();
                break;
            case KeyCode.Home or KeyCode.PageUp:
                // TODO: Add text selection
                SnapBoundsToLeft();
                break;
            case KeyCode.End or KeyCode.PageDown when this.textCursor.TextBoxState.TextLargerThanView:
                // TODO: Add text selection
                SnapBoundsToRight();
                break;
            case KeyCode.End or KeyCode.PageDown:
                SnapBoundsToLeft();
                break;
        }

        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            CurrentCharBounds = this.charBounds[this.textCursor.TextBoxState.CharIndex].bounds,
        };
    }

    private void UpdateTextBoxState()
    {
        this.textSize = this.font.Measure(this.text.ToString());
        this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        {
            TextLength = this.text.Length,
            TextLargerThanView = this.textSize.Width > this.textAreaBounds.Width,
        };
    }

    private void CalcYPositions(float prevHeight, float currentHeight)
    {
        // Calculate the Y position for the text rendering location
        CalcTextPosY(prevHeight, currentHeight);

        // Update all of the character Y positions to have each character centered vertically
        // over the same Y position as the text render location
        for (var i = 0; i < this.charBounds.Count; i++)
        {
            var currItem = this.charBounds[i];

            currItem = currItem with
            {
                bounds = currItem.bounds with
                {
                    Y = this.textPos.Y - currItem.bounds.Height.Half()
                }
            };

            this.charBounds[i] = currItem;
        }
    }

    private void CalcTextPosY(float prevHeight, float currentHeight)
    {
        var increaseInHeight = currentHeight > prevHeight;
        var decreaseInHeight = currentHeight < prevHeight;

        if (prevHeight == 0)
        {
            this.textPos = new Vector2(this.textPos.X, Position.Y - currentHeight.Half() + currentHeight.Half());
        }
        else
        {
            if (increaseInHeight)
            {
                var amount = (float)Math.Round((currentHeight - prevHeight).Half(), MidpointRounding.ToNegativeInfinity);
                this.textPos = new Vector2(this.textPos.X, this.textPos.Y - amount);
            }
            else if (decreaseInHeight)
            {
                var amount = (float)Math.Round((prevHeight - currentHeight).Half(), MidpointRounding.ToNegativeInfinity);
                this.textPos = new Vector2(this.textPos.X, this.textPos.Y + amount);
            }
        }
    }

    private char ToChar(KeyCode key)
    {
        if (KeyboardKeyGroups.LetterKeys.Contains(key))
        {
            return this.currKeyState.AnyShiftKeysDown()
                ? KeyboardKeyGroups.WithShiftLetterKeys[key]
                : KeyboardKeyGroups.NoShiftLetterKeys[key];
        }

        if (KeyboardKeyGroups.StandardNumberKeys.Contains(key))
        {
            return this.currKeyState.AnyShiftKeysDown()
                ? KeyboardKeyGroups.WithShiftStandardNumberCharacters[key]
                : KeyboardKeyGroups.NoShiftStandardNumberCharacters[key];
        }

        if (KeyboardKeyGroups.NumpadNumberKeys.Contains(key) && this.currKeyState.AnyShiftKeysDown() is false)
        {
            return KeyboardKeyGroups.NoShiftNumpadNumberCharacters[key];
        }

        if (KeyboardKeyGroups.SymbolKeys.Contains(key))
        {
            return this.currKeyState.AnyShiftKeysDown()
                ? KeyboardKeyGroups.WithShiftSymbolCharacters[key]
                : KeyboardKeyGroups.NoShiftSymbolCharacters[key];
        }

        return KeyboardKeyGroups.InvalidCharacter;
    }

    private int CalcNextCharIndex(KeyCode key)
    {
        var textLength = this.textCursor.TextBoxState.TextLength;
        var cursorAtRightEndOfText = this.text.IsEmpty() || this.textCursor.Cursor.Right > this.charBounds.TextRight();
        var isAtLastCharIndex = this.textCursor.TextBoxState.CharIndex >= textLength - 1;

        return key switch
        {
            KeyCode.Left => isAtLastCharIndex && cursorAtRightEndOfText
                ? this.textCursor.TextBoxState.CharIndex
                : this.textCursor.TextBoxState.CharIndex - 1,
            KeyCode.Right => this.textCursor.TextBoxState.CharIndex >= textLength - 1
                ? textLength - 1
                : this.textCursor.TextBoxState.CharIndex + 1,
            KeyCode.Home => 0,
            KeyCode.PageUp => 0,
            KeyCode.End => textLength - 1,
            KeyCode.PageDown => textLength - 1,
            _ => this.textCursor.TextBoxState.CharIndex,
        };
    }

    private void UpdateBorder() =>
        this.ctrlBorder = new RectShape
        {
            Position = Position.ToVector2(),
            Color = Color.LightGray,
            Width = Width,
            Height = Height,
            BorderThickness = BorderThickness,
            IsSolid = false,
            CornerRadius = new CornerRadius(5),
        };

    private void RebuildBounds()
    {
        var currentX = this.charBounds.IsEmpty()
            ? this.textAreaBounds.Left
            : this.charBounds.Min(cb => cb.bounds.Left);
        var currentY = this.charBounds.IsEmpty()
            ? Position.Y
            : this.charBounds[0].bounds.Y;

        var newBounds = this.font.GetCharacterBounds(this.text.ToString(), new Vector2(currentX, currentY)).ToArray();
        this.charBounds.Clear();
        this.charBounds.AddRange(newBounds);
    }

    private void SnapBoundsToLeft()
    {
        if (this.charBounds.IsEmpty())
        {
            return;
        }

        var currentY = this.charBounds[0].bounds.Y;
        var newBounds = this.font.GetCharacterBounds(
            this.text.ToString(),
            new Vector2(this.textAreaBounds.Left, currentY)).ToArray();
        this.charBounds.Clear();
        this.charBounds.AddRange(newBounds);
    }

    private void SnapBoundsToRight()
    {
        if (this.charBounds.IsEmpty())
        {
            return;
        }

        var newX = this.textAreaBounds.Right - this.textSize.Width;
        var currentY = this.charBounds[0].bounds.Y;

        var newBounds =
            this.font.GetCharacterBounds(this.text.ToString(), new Vector2(newX, currentY)).ToArray();
        this.charBounds.Clear();
        this.charBounds.AddRange(newBounds);

        var textPastRightEnd = this.charBounds.TextRight() > this.textAreaBounds.Right;

        if (!textPastRightEnd)
        {
            return;
        }

        this.charBounds.BumpAllToleft(this.charBounds.TextRight() - this.textAreaBounds.Right);
    }

    private bool GapAtRightEnd() =>
        this.charBounds.Count > 0 && this.charBounds.TextRight() < this.textAreaBounds.Right;
}
