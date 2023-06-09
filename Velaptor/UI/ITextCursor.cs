// <copyright file="ITextCursor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.UI;

using Graphics;

internal interface ITextCursor
{
    RectShape Cursor { get; set; }

    TextBoxState TextBoxState { get; set; }

    void Update();
}
