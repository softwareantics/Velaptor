// <copyright file="ITexture.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Raptor.Graphics
{
    using System;

    public interface ITexture : IDisposable
    {
        uint ID { get; }

        string Name { get; }

        uint Width { get; }

        uint Height { get; }
    }
}
