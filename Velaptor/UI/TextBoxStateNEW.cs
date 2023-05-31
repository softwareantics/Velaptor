namespace Velaptor.UI;

using System;
using System.Drawing;

internal record struct TextBoxStateNEW
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

    public RectangleF[] CharBounds { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the text is larger than the viewable text area.
    /// </summary>
    public bool TextLargerThanView { get; set; }

    /// <summary>
    /// Gets or sets the bounds of the character at the current cursor location.
    /// </summary>
    public RectangleF CurrentCharBounds { get; set; }

    public Memory<(char character, RectangleF bounds)> CharBoundsFFF { get; set; }
}

/*
 * KeyCode
 * AllTextRight
 * CurrentCharLeft
 * CurrentCharRight
 * CharIndex
 * TextLength
 * TextViewLeft
 * TextViewRight
 * Text (str builder)
 * TextLeft
 * TextRight
 * SelectionMode
 * SelectionHeight
 * TextBoxPosition
 * SelStartCharBounds
 * SelStopCharBounds
 * TextMutateType
 * CursorAtEnd
 * CursorBounds
 *
 *
 * empty
 * lastcharright
 *
 */
