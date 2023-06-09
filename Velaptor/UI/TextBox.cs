// <copyright file="TextBox.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Carbonate.UniDirectional;
using Content.Fonts;
using Factories;
using Graphics;
using Graphics.Renderers;
using Input;
using ReactableData;
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
    private const bool RenderAllText = false;
    private readonly Guid textBoxDataEventId = new ("71931561-826B-431B-BCE6-B139034A1FF4");
    private readonly Guid textCursorDataEventId = new ("31377B4B-3DA1-4133-A2D7-711C6B03455C");
    private readonly IShapeRenderer shapeRenderer;
    private readonly IFontRenderer fontRenderer;
    private readonly ITextSelection textSelection;
    private readonly ITextCursor textCursor;
    private readonly StringBuilder text = new ();
    private readonly List<(char character, RectangleF bounds)> charBounds = new ();
    private readonly IPushReactable<TextBoxStateData> textBoxDataReactable;
    private readonly IPushReactable<TextCursorData> textCursorReactable;
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
    private Color inversedTextColor = Color.Black;
    private Color selectionColor;
    private float maxWidth;
    private uint width = 100;
    private int cursorIndex;
    private TextBoxStateData preTextBoxState;
    private TextBoxStateData postTextBoxState;
    private bool inSelectionMode;
    private int selectionStartIndex;
    private bool selectionAtRightEnd;

    // TODO: Create internal ctor to inject everything for testing
    // TODO: The TextBoxState in the TextCursor is being used for holding state for logic
    // in the TextBox ctrl class.  This is not ideal. Refactor the code to separate these

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBox"/> class.
    /// </summary>
    public TextBox()
    {
        this.textBoxDataReactable = IoC.Container.GetInstance<IPushReactable<TextBoxStateData>>();

        // TODO: TextCursor needs to be created via the IoC container
        this.textCursor = new TextCursor(this.textBoxDataReactable);

        // TODO: TextSelection needs to be created via the IoC container
        this.textSelection = new TextSelection(this.textBoxDataReactable)
        {
            SelectionColor = Color.CornflowerBlue, // TODO: To be removed
        };

        var rendererFactory = new RendererFactory();
        this.shapeRenderer = rendererFactory.CreateShapeRenderer();
        this.fontRenderer = rendererFactory.CreateFontRenderer();
        Keyboard = IoC.Container.GetInstance<IAppInput<KeyboardState>>();
    }

    internal TextBox(
        IPushReactable<TextBoxStateData> textBoxDataReactable,
        IRendererFactory rendererFactory,
        IAppInput<KeyboardState> keyboard,
        IUIDependencyFactory dependencyFactory)
    {
        this.textBoxDataReactable = textBoxDataReactable;

        this.shapeRenderer = rendererFactory.CreateShapeRenderer();
        this.fontRenderer = rendererFactory.CreateFontRenderer();

        this.textCursor = dependencyFactory.CreateTextCursor(this.textBoxDataReactable);
        this.textSelection = dependencyFactory.CreateTextSelection(this.textBoxDataReactable);

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
        }
    }

    public string SelectedText => this.textSelection.SelectedText;

    public Color TextColor { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the color of the text selection rectangle.
    /// </summary>
    public Color SelectionColor
    {
        get => this.selectionColor;
        set
        {
            this.selectionColor = value;
            this.inversedTextColor = value.Inverse();
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

    // TODO: Create a prop for holding the text selection color

    /// <summary>
    /// Loads the content of the <see cref="TextBox"/>.
    /// </summary>
    public override void LoadContent()
    {
        var contentLoader = ContentLoaderFactory.CreateContentLoader();
        this.font = contentLoader.LoadFont("TimesNewRoman-Regular", 12);

        this.maxWidth = this.font.Metrics.Max(m => m.GlyphWidth);

        UpdateBorder();

        // TODO: To be removed
        // this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        // {
        //     Left = Left + MarginLeft,
        //     Right = Right - MarginRight,
        // };

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

        UpdateState(MutateType.PreMutate, false, 0, KeyCode.Unknown, TextBoxEvent.None);
        UpdateState(MutateType.PostMutate, false, 0, KeyCode.Unknown, TextBoxEvent.None);

        // TODO: To be removed
        // this.textSelection.Position = Position.ToVector2();
        // this.textSelection.Height = (int)(Height - MarginTop);

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

        // TODO: To be removed
        // Update the left and right ends of the text viewing area
        // this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        // {
        //     Left = Left + MarginLeft,
        //     Right = Right - MarginRight,
        // };

        // TODO: To be removed
        // var textAreaX = Position.X - Width.Half() + (this.ctrlBorder.BorderThickness + MarginLeft);
        // var textAreaY = Position.Y - Height.Half();
        // var textAreaWidth = Width - (MarginLeft + MarginRight + this.ctrlBorder.BorderThickness);
        // var textAreaHeight = Height;
        // this.textSelection.TextViewArea = new RectangleF(textAreaX, textAreaY, textAreaWidth, textAreaHeight);

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
        if (this.inSelectionMode)
        {
            var textLeft = this.postTextBoxState.TextLeft;
            var textRight = this.postTextBoxState.TextRight;
            var leftEndPastView = this.textSelection.SelectionRect.Left < textLeft;
            var rightEndPastView = this.textSelection.SelectionRect.Right > textRight;

            RectShape selectionRect;

            if (rightEndPastView && !leftEndPastView)
            {
                var amountPastEnd = this.textSelection.SelectionRect.Right - textRight;

                selectionRect = this.textSelection.SelectionRect with
                {
                    Width = this.textSelection.SelectionRect.Width - amountPastEnd,
                    Color = Color.Orange, // debug
                };

                selectionRect.Left -= amountPastEnd.Half();
            }
            else if (leftEndPastView && !rightEndPastView)
            {
                var amountPastEnd = textLeft - this.textSelection.SelectionRect.Left;

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

            if (RenderAllText || inView)
            {
                return glyphMetrics;
            }

            var tempBounds = glyphMetrics.GlyphBounds;
            tempBounds.Width = 0;
            glyphMetrics.GlyphBounds = tempBounds;

            return glyphMetrics;
        }).ToArray().AsSpan();

        // TODO: Need to figure out how to render the text color differently with selected text
        // This will probably require grabbing the 2-3 sections of text that are selected and not selected.
        // Once this is done, the non selected text will render regular clr and the selected text will render
        // with a different clr. There could be normal text, then selected text in the middle, and then normal text again.

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
            this.inSelectionMode = this.currKeyState.AnyShiftKeysDown();
        }

        // When moving into selection mode
        if (this.inSelectionMode && this.prevInSelectionMode is false)
        {
            // TODO: Possibly remove this because it is in the state objects anyways?
            this.selectionStartIndex = this.cursorIndex;

            this.selectionAtRightEnd = this.textCursor.Cursor.Right > this.charBounds.TextRight();
            Debug.WriteLine($"Selection Start Index (In Selection Mode) Index Set To: {this.selectionStartIndex}");
        }

        var textBoxEvent = TextBoxEvent.None;

        if (key.IsVisibleKey())
        {
            textBoxEvent = TextBoxEvent.AddingCharacter;
        }
        else if (KeyboardKeyGroups.DeletionKeys.Contains(key))
        {
            textBoxEvent = string.IsNullOrEmpty(this.textSelection.SelectedText) switch
            {
                true => TextBoxEvent.RemovingSingleChar,
                false => TextBoxEvent.RemovingSelectedChars,
            };
        }
        else if (KeyboardKeyGroups.CursorMovementKeys.Contains(key))
        {
            textBoxEvent = TextBoxEvent.MovingCursor;
        }

        var textRight = this.charBounds.TextRight();

        var hasBounds = this.charBounds.Count > 0;

        UpdateState(MutateType.PreMutate, hasBounds, textRight, key, textBoxEvent);

        if (key.IsVisibleKey())
        {
            Add(key);
        }
        else if (KeyboardKeyGroups.DeletionKeys.Contains(key))
        {
            Remove(key);

            if (this.inSelectionMode)
            {
                this.inSelectionMode = false;
                this.cursorIndex = Math.Min(this.cursorIndex, this.selectionStartIndex);
                this.textSelection.Clear();
            }
        }
        else if (KeyboardKeyGroups.CursorMovementKeys.Contains(key))
        {
            this.cursorVisible = true;

            SetCursorIndex(key);
        }

        textRight = (int)(this.charBounds.IsEmpty() ? this.textAreaBounds.Right : this.charBounds.TextRight());
        hasBounds = this.charBounds.Count > 0;

        UpdateState(MutateType.PostMutate, hasBounds, textRight, key, textBoxEvent);
        this.textCursor.Update();
        this.textSelection.Update();

        // Update the text rendering position
        var charBoundsCenter = this.charBounds.CenterPosition();
        this.textPos = this.textPos with { X = charBoundsCenter.X, };

        this.prevInSelectionMode = this.inSelectionMode;
        base.OnKeyUp(key);
    }

    private void UpdateState(MutateType mutateType, bool hasBounds, int textRight, KeyCode key, TextBoxEvent textBoxEvent)
    {
        RectangleF selStartCharBounds = default;
        RectangleF selStopCharBounds = default;

        if (hasBounds)
        {
            // selStartCharBounds = this.selectionStartIndex == -1 ? default : this.charBounds[this.selectionStartIndex].bounds;
            selStartCharBounds = this.inSelectionMode ? this.charBounds[this.selectionStartIndex].bounds : default;
            selStopCharBounds = this.inSelectionMode ? this.charBounds[this.cursorIndex].bounds : default;
        }

        var newState = new TextBoxStateData
        {
            Text = this.text,
            TextLength = this.text.Length,
            TextLeft = (int)(this.charBounds.IsEmpty() ? this.textAreaBounds.Left : this.charBounds.TextLeft()),
            TextRight = textRight,
            TextViewLeft = (int)this.textAreaBounds.Left,
            TextViewRight = (int)this.textAreaBounds.Right,
            Key = key,
            CharIndex = this.cursorIndex,
            CurrentCharLeft = hasBounds ? (int)this.charBounds[this.cursorIndex].bounds.Left : 0,
            CurrentCharRight = hasBounds ? (int)this.charBounds[this.cursorIndex].bounds.Right : 0,
            InSelectionMode = this.inSelectionMode,
            SelStartCharBounds = selStartCharBounds,
            SelStopCharBounds = selStopCharBounds,
            SelectionStartIndex = this.selectionStartIndex,
            SelectionStopIndex = this.cursorIndex,
            SelectionHeight = (int)(Height - MarginTop),
            SelectionStartedAtRightEnd = this.selectionAtRightEnd,
            Position = Position.ToVector2(),
            CursorAtEnd = this.text.IsEmpty() || this.textCursor.Cursor.Right > textRight,
            // CursorBounds = this.textCursor.Cursor,
            TextMutateType = mutateType,
            Event = textBoxEvent,
        };

        switch (mutateType)
        {
            case MutateType.PreMutate:
                this.preTextBoxState = newState;
                this.textBoxDataReactable.Push(newState, this.textBoxDataEventId);
                break;
            case MutateType.PostMutate:
                this.postTextBoxState = newState;
                this.textBoxDataReactable.Push(newState, this.textBoxDataEventId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutateType), mutateType, null);
        }
    }

    private void Add(KeyCode key)
    {
        var character = key == KeyCode.Space
            ? " "
            : ToChar(key).ToString();

        var charIndexAtEnd = this.cursorIndex >= this.text.Length - 1;

        var cursorAtEnd = this.preTextBoxState.CursorAtEnd;

        var insertIndex = charIndexAtEnd && cursorAtEnd
            ? this.text.Length
            : this.cursorIndex;

        var prevHeight = this.textSize.Height;

        this.text.Insert(insertIndex, character);
        this.textSize = this.font.Measure(this.text.ToString());
        RebuildBounds();

        var textLargerThanView = this.charBounds.TextWidth() > this.textAreaBounds.Width;

        if (textLargerThanView)
        {
            if (charIndexAtEnd)
            {
                SnapBoundsToRight();
            }
            else
            {
                var cursorWillBePastRightEnd =
                    this.charBounds[this.cursorIndex].bounds.Right > this.textAreaBounds.Right;

                if (cursorWillBePastRightEnd)
                {
                    var overLap = this.charBounds[this.cursorIndex].bounds.Right - this.textAreaBounds.Right;
                    this.charBounds.BumpAllToleft(overLap);
                }
            }
        }
        else
        {
            if (charIndexAtEnd && cursorAtEnd)
            {
                SnapBoundsToLeft();
            }
        }

        CalcYPositions(prevHeight, this.textSize.Height);

        this.cursorIndex = charIndexAtEnd
            ? this.text.Length - 1
            : this.cursorIndex + 1;
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
        var charIndexAtStart = this.cursorIndex <= 0;
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
            this.text.RemoveChar(this.cursorIndex - 1);
        }

        this.textSize = this.font.Measure(this.text.ToString());
        RebuildBounds();

        if (this.text.IsEmpty())
        {
            this.charBounds.Clear();
        }

        this.cursorIndex = cursorAtRightEndOfText
            ? Math.Clamp(this.text.Length - 1, 0, int.MaxValue)
            : this.cursorIndex - 1;

        // Update the cursor index

        // TODO: Remove
        // this.textCursor.TextBoxState = this.textCursor.TextBoxState with { CharIndex = cursorIndex, };

        var prevCharIndexAtStart = this.cursorIndex - 1 <= 0;
        var prevCharIndexNotAtStart = !prevCharIndexAtStart;
        var textLargerThanView = this.charBounds.TextWidth() > this.textAreaBounds.Width;
        var gapAtRightEnd = GapAtRightEnd();
        var gapNotAtRightEnd = !gapAtRightEnd;

        switch (textLargerThanView)
        {
            case true when gapAtRightEnd:
                SnapBoundsToRight();
                break;
            case true when gapNotAtRightEnd && prevCharIndexNotAtStart:
                var bumpAmount = this.textAreaBounds.Left + MarginLeft - this.charBounds[this.cursorIndex - 1].bounds.Left;
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

        // TODO: To be removed
        // this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        // {
        //     CurrentCharBounds = this.charBounds.IsEmpty()
        //         ? this.textCursor.TextBoxState.CurrentCharBounds
        //         : this.charBounds[this.cursorIndex].bounds,
        // };

        CalcYPositions(prevHeight, this.textSize.Height);
    }

    private void DeleteCurrentChar()
    {
        var charIndexAtStart = this.cursorIndex <= 0;
        var charIndexAtEnd = this.cursorIndex >= this.text.Length - 1;
        var cursorAtRightEndOfText = this.preTextBoxState.CursorAtEnd;

        if (cursorAtRightEndOfText)
        {
            return;
        }

        var prevHeight = this.textSize.Height;

        this.text.RemoveChar(this.cursorIndex);
        this.textSize = this.font.Measure(this.text.ToString());
        RebuildBounds();

        var charIndexNotAtStart = !charIndexAtStart;

        var textLargerThanView = this.charBounds.TextWidth() > this.textAreaBounds.Width;

        if (textLargerThanView)
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

        this.cursorIndex = charIndexAtEnd
            ? this.cursorIndex - 1
            : this.cursorIndex;

        CalcYPositions(prevHeight, this.textSize.Height);
    }

    private void RemoveSelectedChars()
    {
        if (!this.inSelectionMode)
        {
            return;
        }

        var selectionToEndOfText = this.textSelection.SelectionRect.Right >= this.charBounds.TextRight();

        // TODO: To be removed
        // var startCharIndex = Math.Min(this.textSelection.SelectionStartIndex, this.textSelection.SelectionStopIndex);
        // var stopCharIndex = selectionToEndOfText
        //     ? Math.Max(this.textSelection.SelectionStartIndex, this.textSelection.SelectionStopIndex) + 1
        //     : Math.Max(this.textSelection.SelectionStartIndex, this.textSelection.SelectionStopIndex);

        var startCharIndex = Math.Min(this.selectionStartIndex, this.cursorIndex);
        var stopCharIndex = selectionToEndOfText
            ? Math.Max(this.selectionStartIndex, this.cursorIndex) + 1
            : Math.Max(this.selectionStartIndex, this.cursorIndex);

        var removeLength = Math.Abs(startCharIndex - stopCharIndex);
        var prevHeight = this.textSize.Height;

        this.text.Remove(startCharIndex, removeLength);

        this.textSize = this.font.Measure(this.text.ToString());
        RebuildBounds();
        // this.textSelection.Clear(); // TODO: To be removed

        var textLargerThanView = this.charBounds.TextWidth() > this.textAreaBounds.Width;

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

        this.cursorIndex = startCharIndex >= this.text.Length - 1
            ? this.text.Length - 1
            : startCharIndex;

        // TODO: To be removed
        // this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        // {
        //     // CharIndex = cursorIndex,
        //     CurrentCharBounds = this.text.IsEmpty()
        //         ? default
        //         : this.charBounds[this.cursorIndex].bounds,
        // };

        CalcYPositions(prevHeight, this.textSize.Height);
    }

    private void AdjustBoundsAfterMovingLeft()
    {
        var charIndexAtStart = this.cursorIndex <= 0;

        // If currently at the first char index
        if (charIndexAtStart)
        {
            SnapBoundsToLeft();
        }
        else
        {
            var currentCharLeft = this.charBounds.CharLeft(this.cursorIndex);
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

    private void AdjustBoundsAfterMovingRight()
    {
        var textNotLargerThanView = this.charBounds.TextWidth() <= this.textAreaBounds.Width;

        if (textNotLargerThanView)
        {
            return;
        }

        var currentCharRight = this.charBounds.CharRight(this.cursorIndex);
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

        this.cursorIndex = CalcNextCharIndex(key);

        // TODO: To be removed
        // this.textCursor.TextBoxState = this.textCursor.TextBoxState with { CharIndex = newIndex, };
        // this.textSelection.SelectionStopIndex = newIndex;

        var textLargerThanView = this.charBounds.TextWidth() > this.textAreaBounds.Width;

        switch (key)
        {
            case KeyCode.Left:
                AdjustBoundsAfterMovingLeft();
                break;
            case KeyCode.Right:
                AdjustBoundsAfterMovingRight();
                break;
            case KeyCode.Home or KeyCode.PageUp:
                // TODO: Add text selection
                SnapBoundsToLeft();
                break;
            case KeyCode.End or KeyCode.PageDown when textLargerThanView:
                // TODO: Add text selection
                SnapBoundsToRight();
                break;
            case KeyCode.End or KeyCode.PageDown:
                SnapBoundsToLeft();
                break;
        }


        // TODO: To be removed
        // this.textCursor.TextBoxState = this.textCursor.TextBoxState with
        // {
        //     CurrentCharBounds = this.charBounds[this.cursorIndex].bounds,
        // };
    }

    // TODO: Convert to extension method and create tests
    private void CalcYPositions(float prevHeight, float currentHeight)
    {
        // Calculate the Y position for the text rendering location
        // TODO: This can become a local function
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

    // TODO: Convert to extension method and create tests
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
        var textLength = this.text.Length;
        var cursorAtRightEndOfText = this.text.IsEmpty() || this.textCursor.Cursor.Right > this.charBounds.TextRight();
        var isAtLastCharIndex = this.cursorIndex >= textLength - 1;

        var newIndex = key switch
        {
            KeyCode.Left => isAtLastCharIndex && cursorAtRightEndOfText
                ? this.cursorIndex
                : this.cursorIndex - 1,
            KeyCode.Right => this.cursorIndex >= textLength - 1
                ? textLength - 1
                : this.cursorIndex + 1,
            KeyCode.Home => 0,
            KeyCode.PageUp => 0,
            KeyCode.End => textLength - 1,
            KeyCode.PageDown => textLength - 1,
            _ => this.cursorIndex,
        };

        return newIndex < 0 ? 0 : newIndex;
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
