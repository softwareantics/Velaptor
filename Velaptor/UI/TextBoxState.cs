// <copyright file="TextBoxState.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using System.Drawing;

/// <summary>
/// Represents the state of a <see cref="TextBox"/>.
/// </summary>
internal record struct TextBoxState
{
    private readonly int charIndex;

    /// <summary>
    /// Gets the index of the cursor.
    /// </summary>
    public int CharIndex
    {
        readonly get => this.charIndex;
        init => this.charIndex = value < 0 ? 0 : value;
    }

    /// <summary>
    /// Gets the length of the text.
    /// </summary>
    public int TextLength { get; init; }

    /// <summary>
    /// Gets or sets the X position of the left side of the text box.
    /// </summary>
    public int Left { get; set; }

    /// <summary>
    /// Gets or sets the X position of the right side of the text box.
    /// </summary>
    public int Right { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the text is larger than the viewable text area.
    /// </summary>
    public bool TextLargerThanView { get; set; }

    /// <summary>
    /// Gets or sets the bounds of the character at the current cursor location.
    /// </summary>
    public RectangleF CurrentCharBounds { get; set; }
}
