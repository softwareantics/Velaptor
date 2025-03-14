﻿// <copyright file="AnimatedGraphicsScene.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTesting.Scenes;

using System.Drawing;
using Velaptor.Scene;
using Velaptor;
using Velaptor.Content;
using Velaptor.Factories;
using Velaptor.Graphics;
using Velaptor.Graphics.Renderers;
using Velaptor.UI;

/// <summary>
/// Tests that animated graphics properly render to the screen.
/// </summary>
public class AnimatedGraphicsScene : SceneBase
{
    private const int TopMargin = 50;
    private IAtlasData? mainAtlas;
    private ITextureRenderer? textureRenderer;
    private AtlasSubTextureData[]? frames;
    private Label? lblInstructions;
    private int elapsedTime;
    private int currentFrame;

    /// <inheritdoc cref="IScene.LoadContent"/>
    public override void LoadContent()
    {
        if (IsLoaded)
        {
            return;
        }

        var renderFactory = new RendererFactory();

        this.textureRenderer = renderFactory.CreateTextureRenderer();

        this.mainAtlas = ContentLoader.LoadAtlas("Main-Atlas");
        this.frames = this.mainAtlas.GetFrames("circle");

        this.lblInstructions = new Label();
        this.lblInstructions.Text = "Verify that the Kinson Digital logo is rotating clockwise.";
        this.lblInstructions.Color = Color.White;

        AddControl(this.lblInstructions);

        this.lblInstructions.Left = WindowCenter.X - (int)(this.lblInstructions.Width / 2);
        this.lblInstructions.Top = TopMargin;

        base.LoadContent();
    }

    /// <inheritdoc cref="IScene.UnloadContent"/>
    public override void UnloadContent()
    {
        if (!IsLoaded || IsDisposed)
        {
            return;
        }

        ContentLoader.UnloadAtlas(this.mainAtlas);

        base.UnloadContent();
    }

    /// <inheritdoc cref="IUpdatable.Update"/>
    public override void Update(FrameTime frameTime)
    {
        if (this.elapsedTime >= 124)
        {
            this.elapsedTime = 0;

            this.currentFrame = this.currentFrame >= this.frames.Length - 1
                ? 0
                : this.currentFrame + 1;
        }
        else
        {
            this.elapsedTime += frameTime.ElapsedTime.Milliseconds;
        }
    }

    /// <inheritdoc cref="IDrawable.Render"/>
    public override void Render()
    {
        var posX = WindowCenter.X - (this.frames[this.currentFrame].Bounds.Width / 2);
        var posY = WindowCenter.Y - (this.frames[this.currentFrame].Bounds.Height / 2);

        this.textureRenderer.Render(
            this.mainAtlas.Texture,
            this.frames[this.currentFrame].Bounds,
            new Rectangle(posX, posY, (int)this.mainAtlas.Width, (int)this.mainAtlas.Height),
            1f,
            0f,
            Color.White,
            RenderEffects.None);
        base.Render();
    }

    /// <inheritdoc cref="SceneBase.Dispose(bool)"/>
    protected override void Dispose(bool disposing)
    {
        if (IsDisposed || !IsLoaded)
        {
            return;
        }

        base.Dispose(disposing);
    }
}
