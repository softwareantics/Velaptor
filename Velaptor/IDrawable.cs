// <copyright file="IDrawable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor
{
    // ReSharper disable RedundantNameQualifier
    using Velaptor.Graphics;

    // ReSharper restore RedundantNameQualifier

    /// <summary>
    /// Provides the ability for an object to be rendered.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Renders the object.
        /// </summary>
        /// <param name="spriteBatch">Renders textures, primitives, and text.</param>
        void Render(ISpriteBatch spriteBatch);
    }
}
