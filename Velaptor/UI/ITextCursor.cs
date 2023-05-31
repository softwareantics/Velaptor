// <copyright file="ITextCursor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using Graphics;
using Input;

internal interface ITextCursor
{
    RectShape Cursor { get; set; }

    TextBoxState TextBoxState { get; set; }

    void PreTextMutate();

    void PostTextMutate();

    TextBoxState UpdateCursorState(TextBoxState state);

    void AdjustCursor(KeyCode key, TextBoxEvent textBoxEvent);
}
