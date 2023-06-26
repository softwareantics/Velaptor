﻿// <copyright file="Enums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTesting;

/// <summary>
/// The different layers for textures to be rendered for testing purposes.
/// </summary>
public enum RenderLayer
{
    /// <summary>
    /// Layer 1.
    /// </summary>
    One = -10, // White default layer (White changes layers)

    /// <summary>
    /// Layer 2.
    /// </summary>
    Two = 10, // Orange layer

    /// <summary>
    /// Layer 3.
    /// </summary>
    Three = 20, // Free spot

    /// <summary>
    /// Layer 4.
    /// </summary>
    Four = 30, // Blue layer

    /// <summary>
    /// Layer 5.
    /// </summary>
    Five = 40, // Free spot
}

/// <summary>
/// Different kinds of shapes.
/// </summary>
public enum ShapeType
{
    /// <summary>
    /// A rectangle.
    /// </summary>
    Rectangle = 1,

    /// <summary>
    /// A circle.
    /// </summary>
    Circle = 2,
}

public enum TextBoxSetting
{
    TextColor = 0,

    MoveTextBox = 1,

    TextBoxWidth = 2,

    FontChange = 3,
}

public enum ColorComponent
{
    Red,

    Green,

    Blue,
}
