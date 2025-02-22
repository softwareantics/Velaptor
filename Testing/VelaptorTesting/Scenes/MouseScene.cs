﻿// <copyright file="MouseScene.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTesting.Scenes;

using System;
using System.Drawing;
using Velaptor.Scene;
using Velaptor;
using Velaptor.Factories;
using Velaptor.Input;
using Velaptor.UI;

/// <summary>
/// Used to test that the mouse works correctly.
/// </summary>
public class MouseScene : SceneBase
{
    private IAppInput<MouseState>? mouse;
    private Label? mouseInfoLabel;
    private MouseScrollDirection scrollDirection;

    /// <inheritdoc cref="IScene.LoadContent"/>
    public override void LoadContent()
    {
        if (IsLoaded)
        {
            return;
        }

        this.mouse = InputFactory.CreateMouse();
        this.mouseInfoLabel = new Label { Color = Color.White };

        this.mouseInfoLabel.LoadContent();
        this.mouseInfoLabel.Position = new Point(WindowCenter.X, WindowCenter.Y);

        AddControl(this.mouseInfoLabel);

        base.LoadContent();
    }

    /// <inheritdoc cref="IUpdatable.Update"/>
    public override void Update(FrameTime frameTime)
    {
        var currentMouseState = this.mouse.GetState();

        var mouseInfo = $"Mouse Position: {currentMouseState.GetX()}, {currentMouseState.GetY()}";
        mouseInfo += $"{Environment.NewLine}Left Button: {(currentMouseState.IsLeftButtonDown() ? "Down" : "Up")}";
        mouseInfo += $"{Environment.NewLine}Right Button: {(currentMouseState.IsRightButtonDown() ? "Down" : "Up")}";
        mouseInfo += $"{Environment.NewLine}Middle Button: {(currentMouseState.IsMiddleButtonDown() ? "Down" : "Up")}";

        if (currentMouseState.GetScrollWheelValue() != 0)
        {
            this.scrollDirection = currentMouseState.GetScrollDirection();
        }

        mouseInfo += $"{Environment.NewLine}Mouse Scroll Direction: {this.scrollDirection}";

        this.mouseInfoLabel.Text = mouseInfo;

        base.Update(frameTime);
    }

    public override void UnloadContent()
    {
        if (!IsLoaded || IsDisposed)
        {
            return;
        }

        this.scrollDirection = MouseScrollDirection.None;
        this.mouseInfoLabel = null;
        this.mouse = default;

        base.UnloadContent();
    }
}
