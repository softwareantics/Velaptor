// <copyright file="Window.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Raptor
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using Raptor.Content;
    using Raptor.OpenGL;
    using Silk.NET.OpenGL;
    using Silk.NET.Windowing.Common;
    using GLWindow = Silk.NET.Windowing.Window;
    using IGLWindow = Silk.NET.Windowing.Common.IWindow;

    //TODO: Create an exception that can be thrown when a user tries to create
    /// <summary>
    /// A system window that graphics can be rendered to.
    /// </summary>
    public class Window : IDisposable
    {
        private bool isDisposed;
        private IGLInvoker? gl;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        /// <param name="window">The window implementation that contains the window functionality.</param>
        /// <param name="contentLoader">Loads content.</param>
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception messages only used inside constructor.")]
        public Window(IContentLoader? contentLoader)
        {
            /*TODO: Need to use think about using the DeferredActionsCollection class.  This will
             * make it much easier to defer execution of creating the gl invoker until the GLWindow has
             * fully ran.  This will make sure that the GL context has been created first before making any GL calls.
             */
            throw new Exception("Not Fully Implemented Exception");

            if (WindowInstance is null)
                throw new ArgumentNullException(nameof(WindowInstance), "Window must not be null.");

            if (contentLoader is null)
                throw new ArgumentNullException(nameof(contentLoader), "Content loader must not be null.");

            this.gl = new SilkInvoker();

            InitWindow(800, 600);
            ContentLoader = contentLoader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        /// <param name="contentLoader">Loads content.</param>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception messages only used inside constructor.")]
        public Window(IGLInvoker gl, IContentLoader contentLoader, int width = 800, int height = 600)
        {
            /*TODO: Need to use think about using the DeferredActionsCollection class.  This will
             * make it much easier to defer execution of creating the gl invoker until the GLWindow has
             * fully ran.  This will make sure that the GL context has been created first before making any GL calls.
             */
            throw new Exception("Not Fully Implemented Exception");

            if (contentLoader is null)
                throw new ArgumentNullException(nameof(contentLoader), "Content loader must not be null.");

            ContentLoader = contentLoader;
            InitWindow(width, height);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        /// <param name="window">The internal window implementation that manages a window.</param>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message only used inside of constructor.")]
        [ExcludeFromCodeCoverage]
        public Window(int width = 800, int height = 600) => InitWindow(width, height);

        /// <summary>
        /// Gets the instance of the window to be used in create an instance of the GL API.
        /// </summary>
        public static IWindow? WindowInstance { get; private set; }

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        public static string Title
        {
            get => WindowInstance is null ? string.Empty : WindowInstance.Title;
            set
            {
                if (WindowInstance is null)
                    return;

                WindowInstance.Title = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the window.
        /// </summary>
        public static int Width
        {
            get => WindowInstance is null ? 0 : WindowInstance.Size.Width;
            set
            {
                if (WindowInstance is null)
                    return;

                WindowInstance.Size = new Size(value, WindowInstance.Size.Height);
            }
        }

        /// <summary>
        /// Gets or sets the height of the window.
        /// </summary>
        public static int Height
        {
            get => WindowInstance is null ? 0 : WindowInstance.Size.Height;
            set
            {
                if (WindowInstance is null)
                    return;

                WindowInstance.Size = new Size(WindowInstance.Size.Width, value);
            }
        }

        /// <summary>
        /// Gets or sets the frequency of how often the window updates and draws
        /// in hertz.
        /// </summary>
        public static double UpdateFrequency
        {
            get => WindowInstance is null ? 0 : WindowInstance.UpdatesPerSecond;
            set
            {
                if (WindowInstance is null)
                    return;

                WindowInstance.UpdatesPerSecond = value;
            }
        }

        /// <summary>
        /// Gets the content loader for loading content.
        /// </summary>
        public IContentLoader? ContentLoader { get; private set; }

        /// <summary>
        /// Shows the window.
        /// </summary>
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message only used inside of method.")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Requires instance access for library users.")]
        public void Show()
        {
            if (WindowInstance is null)
                throw new Exception("Internal window implementation not set.");

            WindowInstance.Run();
        }

        /// <summary>
        /// Invoked when the window is loaded.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual void OnLoad()
        {
        }

        /// <summary>
        /// Invoked when the window is updated.
        /// </summary>
        /// <param name="frameTime">The amount of time since the last frame.</param>
        [ExcludeFromCodeCoverage]
        public virtual void OnUpdate(FrameTime frameTime)
        {
        }

        /// <summary>
        /// Invoked when the window renders its content.
        /// </summary>
        /// <param name="frameTime">The amount of time since the last frame.</param>
        [ExcludeFromCodeCoverage]
        public virtual void OnDraw(FrameTime frameTime)
        {
        }

        /// <summary>
        /// Invoked when the window size changes.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual void OnResize()
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True to dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    if (!(WindowInstance is null))
                    {
                        WindowInstance.Update -= Window_Update;
                        WindowInstance.Render -= Window_Render;
                        WindowInstance?.Dispose();
                    }
                }

                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Initializes the window.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        [ExcludeFromCodeCoverage]
        private void InitWindow(int width, int height)
        {
            var options = WindowOptions.Default;
            options.Size = new Size(width, height);

            WindowInstance = GLWindow.Create(options);
            WindowInstance.Load += Window_Load;
            WindowInstance.Update += Window_Update;
            WindowInstance.Render += Window_Render;
            WindowInstance.Resize += Window_Resize;
        }

        /// <summary>
        /// Updates the viewport when the size of the window changes.
        /// </summary>
        /// <param name="size">The current size of the window.</param>
        private void Window_Resize(Size size)
        {
            /*TODO:
             * Currently the rendered textures are scewed.  This is due to the sprite batch not being updated
             * for the window width and height.  Find a way for the sprite batch to get those values updated
             * so the textures can be scalled appropriatly.
             */
            //TODO: Look into throwing exception if null instead unless DI can be properly figured out for GL api creation
            this.gl?.ViewPort(size);
            OnResize();
        }

        /// <summary>
        /// Sets up GL debug callback, inits the silk invoker and invokes the <see cref="OnLoad"/>() method.
        /// </summary>
        private void Window_Load()
        {
            this.gl = new SilkInvoker();

            this.gl.Enable(EnableCap.DebugOutput);
            this.gl.Enable(EnableCap.DebugOutputSynchronous);
            this.gl.DebugCallback(DebugCallback);

            ContentLoader = new ContentLoader();

            OnLoad();
        }

        /// <summary>
        /// Invokes the <see cref="OnUpdate(FrameTime)"/> method.
        /// </summary>
        /// <param name="time">The delta time.</param>
        private void Window_Update(double time) => OnUpdate(new FrameTime() { ElapsedTime = time * 1000.0 });

        /// <summary>
        /// Clears the screen and invokes the <see cref="OnDraw(FrameTime)"/> method.
        /// </summary>
        /// <param name="time">The delta time.</param>
        private void Window_Render(double time)
        {
            //TODO: Look into throwing exception if null instead unless DI can be properly figured out for GL api creation
            this.gl?.Clear();

            OnDraw(new FrameTime() { ElapsedTime = time * 1000.0 });
        }

        /// <summary>
        /// Invokes when there are GL related exceptions that occur.
        /// </summary>
        /// <param name="source">The source of the message.</param>
        /// <param name="type">The type of message.</param>
        /// <param name="id">The message ID.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="length">The length of the message.</param>
        /// <param name="message">The message itself.</param>
        /// <param name="userParam">Custom user data.</param>
        private void DebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userParam)
        {
            var errorMessage = Marshal.PtrToStringAnsi(message);

            errorMessage += errorMessage;
            errorMessage += $"\n\tSrc: {source}";
            errorMessage += $"\n\tType: {type}";
            errorMessage += $"\n\tID: {id}";
            errorMessage += $"\n\tSeverity: {severity}";
            errorMessage += $"\n\tLength: {length}";
            errorMessage += $"\n\tUser Param: {Marshal.PtrToStringAnsi(userParam)}";

            if (severity != GLEnum.DebugSeverityNotification)
                throw new Exception(errorMessage);
        }
    }
}
