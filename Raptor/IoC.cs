// <copyright file="IoC.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Raptor
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using FileIO.Core;
    using FileIO.File;
    using Raptor.Graphics;
    using Raptor.OpenGL;
    using Silk.NET.Windowing;
    using Silk.NET.Windowing.Common;
    using SimpleInjector;
    using SimpleInjector.Diagnostics;
    using SilkWindow = Silk.NET.Windowing.Window;

    /// <summary>
    /// Provides dependency injection for the applcation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class IoC
    {
        private static readonly Container IocContainer = new Container();
        private static bool isInitialized;

        /// <summary>
        /// Gets the inversion of control container used to get instances of objects.
        /// </summary>
        public static Container Container
        {
            get
            {
                if (!isInitialized)
                    SetupContainer();

                return IocContainer;
            }
        }

        /// <summary>
        /// Sets up the IoC container.
        /// </summary>
        private static void SetupContainer()
        {
            IocContainer.Register<ITextFile, TextFile>();
            IocContainer.Register<IImageFile, ImageFile>();
            IocContainer.Register<IGLInvoker>(() =>
            {
                if (Window.WindowInstance is null)
                    throw new Exception($"The '{nameof(Window.WindowInstance)}' must not be null.");

                return new SilkInvoker(Window.WindowInstance);
            }, Lifestyle.Singleton);

            IocContainer.Register<IGPUBuffer, GPUBuffer<VertexData>>();

            var bufferRegistration = IocContainer.GetRegistration(typeof(IGPUBuffer))?.Registration;
            bufferRegistration?.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "asdf");

            IocContainer.Register<IShaderProgram, ShaderProgram>();
            var shaderRegistration = IocContainer.GetRegistration(typeof(IShaderProgram))?.Registration;
            shaderRegistration?.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "asdf");

            IocContainer.Register<ISpriteBatch, SpriteBatch>();
            var spriteBatchRegistration = IocContainer.GetRegistration(typeof(ISpriteBatch))?.Registration;
            spriteBatchRegistration?.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "asdf");

            isInitialized = true;
        }
    }
}
