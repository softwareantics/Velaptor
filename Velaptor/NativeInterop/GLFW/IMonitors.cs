// <copyright file="IMonitors.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.NativeInterop.GLFW;

using System;
using Hardware;

/// <summary>
/// Represents multiple monitors in a system.
/// </summary>
internal interface IMonitors : IDisposable
{
    /// <summary>
    /// Gets a list of all the monitors currently in the system.
    /// </summary>
    SystemMonitor[] SystemMonitors { get; }

    /// <summary>
    /// Refreshes the monitor information.
    /// </summary>
    void Refresh();
}
