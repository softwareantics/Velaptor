// <copyright file="TextCursorData.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.ReactableData;

public readonly record struct TextCursorData
{
    public bool CursorAtEnd { get; init; }
}
