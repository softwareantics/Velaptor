﻿// <copyright file="RendererFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Factories;

using System.Diagnostics.CodeAnalysis;
using Batching;
using Graphics.Renderers;
using NativeInterop.OpenGL;
using OpenGL.Batching;
using OpenGL.Buffers;

/// <inheritdoc/>
[ExcludeFromCodeCoverage(Justification = "Cannot unit test due direct interaction with IoC container.")]
public sealed class RendererFactory : IRendererFactory
{
    private static ITextureRenderer? textureRenderer;
    private static IFontRenderer? fontRenderer;
    private static IShapeRenderer? shapeRenderer;
    private static ILineRenderer? lineRenderer;

    /// <inheritdoc/>
    public ITextureRenderer CreateTextureRenderer()
    {
        if (textureRenderer is not null)
        {
            return textureRenderer;
        }

        var glInvoker = IoC.Container.GetInstance<IGLInvoker>();
        var reactableFactory = IoC.Container.GetInstance<IReactableFactory>();
        var openGLService = IoC.Container.GetInstance<IOpenGLService>();
        var buffer = IoC.Container.GetInstance<IGPUBuffer<TextureBatchItem>>();
        var shader = IoC.Container.GetInstance<IShaderFactory>().CreateTextureShader();
        var batchManager = IoC.Container.GetInstance<IBatchingManager>();

        textureRenderer = new TextureRenderer(
            glInvoker,
            reactableFactory,
            openGLService,
            buffer,
            shader,
            batchManager);

        return textureRenderer;
    }

    /// <inheritdoc/>
    public IFontRenderer CreateFontRenderer()
    {
        if (fontRenderer is not null)
        {
            return fontRenderer;
        }

        var glInvoker = IoC.Container.GetInstance<IGLInvoker>();
        var reactableFactory = IoC.Container.GetInstance<IReactableFactory>();
        var openGLService = IoC.Container.GetInstance<IOpenGLService>();
        var buffer = IoC.Container.GetInstance<IGPUBuffer<FontGlyphBatchItem>>();
        var shader = IoC.Container.GetInstance<IShaderFactory>().CreateFontShader();
        var batchManager = IoC.Container.GetInstance<IBatchingManager>();

        fontRenderer = new FontRenderer(
            glInvoker,
            reactableFactory,
            openGLService,
            buffer,
            shader,
            batchManager);

        return fontRenderer;
    }

    /// <inheritdoc/>
    public IShapeRenderer CreateShapeRenderer()
    {
        if (shapeRenderer is not null)
        {
            return shapeRenderer;
        }

        var glInvoker = IoC.Container.GetInstance<IGLInvoker>();
        var reactableFactory = IoC.Container.GetInstance<IReactableFactory>();
        var openGLService = IoC.Container.GetInstance<IOpenGLService>();
        var buffer = IoC.Container.GetInstance<IGPUBuffer<ShapeBatchItem>>();
        var shader = IoC.Container.GetInstance<IShaderFactory>().CreateRectShader();
        var batchManager = IoC.Container.GetInstance<IBatchingManager>();

        shapeRenderer = new ShapeRenderer(
            glInvoker,
            reactableFactory,
            openGLService,
            buffer,
            shader,
            batchManager);

        return shapeRenderer;
    }

    /// <inheritdoc/>
    public ILineRenderer CreateLineRenderer()
    {
        if (lineRenderer is not null)
        {
            return lineRenderer;
        }

        var glInvoker = IoC.Container.GetInstance<IGLInvoker>();
        var reactableFactory = IoC.Container.GetInstance<IReactableFactory>();
        var openGLService = IoC.Container.GetInstance<IOpenGLService>();
        var buffer = IoC.Container.GetInstance<IGPUBuffer<LineBatchItem>>();
        var shader = IoC.Container.GetInstance<IShaderFactory>().CreateLineShader();
        var batchManager = IoC.Container.GetInstance<IBatchingManager>();

        lineRenderer = new LineRenderer(
            glInvoker,
            reactableFactory,
            openGLService,
            buffer,
            shader,
            batchManager);

        return lineRenderer;
    }
}
