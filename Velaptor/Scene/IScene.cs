﻿// <copyright file="IScene.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Scene;

using System;
using System.Drawing;
using Content;
using Velaptor;
using UI;

/// <summary>
/// Represents a single scene that can be rendered to the screen.
/// </summary>
public interface IScene : IUpdatable, IDrawable, IDisposable
{
    /// <summary>
    /// Gets the name of the scene.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the unique ID of the scene.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets a value indicating whether or not the scene has been loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets the size of the window.
    /// </summary>
    SizeU WindowSize { get; }

    /// <summary>
    /// Gets the center of the window.
    /// </summary>
    Point WindowCenter { get; }

    /// <summary>
    /// Gets the content loader.
    /// </summary>
    IContentLoader ContentLoader { get; }

    /// <summary>
    /// Loads the scene content.
    /// </summary>
    void LoadContent();

    /// <summary>
    /// Adds a control to the scene to be updated and rendered.
    /// </summary>
    /// <param name="control">The control to add to the scene.</param>
    void AddControl(IControl control);

    /// <summary>
    /// Removes the given <paramref name="control"/> from the scene.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    void RemoveControl(IControl control);

    /// <summary>
    /// Unloads the scene's content.
    /// </summary>
    void UnloadContent();
}
