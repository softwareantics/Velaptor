// <copyright file="GLFWMonitors.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.NativeInterop.GLFW;

using System;
using System.Collections.Generic;
using System.Numerics;
using Guards;
using Hardware;

/// <summary>
/// Gets all of the monitors in the system.
/// </summary>
internal sealed class GLFWMonitors : IMonitors
{
    private readonly bool glfwInitialized;
    private readonly IGLFWInvoker glfwInvoker;
    private readonly IPlatform platform;
    private readonly List<SystemMonitor> monitors = new ();
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GLFWMonitors"/> class.
    /// </summary>
    /// <param name="glfwInvoker">Invokes GLFW functions.</param>
    /// <param name="platform">Provides information about the current platform.</param>
    public GLFWMonitors(IGLFWInvoker glfwInvoker, IPlatform platform)
    {
        EnsureThat.ParamIsNotNull(glfwInvoker);
        EnsureThat.ParamIsNotNull(platform);

        this.glfwInvoker = glfwInvoker;
        this.platform = platform;

        if (!this.glfwInitialized)
        {
            this.glfwInvoker.Init();
            this.glfwInitialized = true;
        }

        Refresh();

        this.glfwInvoker.OnMonitorChanged += GLFWInvoker_OnMonitorChanged;
    }

    /// <inheritdoc/>
    public SystemMonitor[] SystemMonitors => this.monitors.ToArray();

    /// <inheritdoc/>
    public void Refresh()
    {
        Vector2 GetMonitorScale(nint monitorHandle)
        {
            var scale = this.glfwInvoker.GetMonitorContentScale(monitorHandle);

            return new Vector2(scale.X, scale.Y);
        }

        var monitorHandles = this.glfwInvoker.GetMonitors();

        foreach (var monitorHandle in monitorHandles)
        {
            var monitorVideoMode = this.glfwInvoker.GetVideoMode(monitorHandle);

            var monitorScale = GetMonitorScale(monitorHandle);

            var newMonitor = new SystemMonitor(this.platform)
            {
                IsMain = this.monitors.Count <= 0,
                RedBitDepth = monitorVideoMode.RedBits,
                BlueBitDepth = monitorVideoMode.BlueBits,
                GreenBitDepth = monitorVideoMode.GreenBits,
                Height = monitorVideoMode.Height,
                Width = monitorVideoMode.Width,
                RefreshRate = monitorVideoMode.RefreshRate,
                HorizontalScale = monitorScale.X,
                VerticalScale = monitorScale.Y,
            };

            this.monitors.Add(newMonitor);
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose() => Dispose(true);

    /// <summary>
    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// </summary>
    /// <param name="disposing">Disposes managed resources when <c>true</c>.</param>
    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            this.glfwInvoker.OnMonitorChanged -= GLFWInvoker_OnMonitorChanged;
        }

        this.isDisposed = true;
    }

    /// <summary>
    /// Occurs when a monitor is connected or disconnected.
    /// </summary>
    private void GLFWInvoker_OnMonitorChanged(object? sender, GLFWMonitorChangedEventArgs e)
        => Refresh();
}
