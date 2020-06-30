// <copyright file="SilkInvoker.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Raptor.OpenGL
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using Silk.NET.OpenGL;
    using Silk.NET.Windowing.Common;
    using RaptorWindow = Raptor.Window;

    /// <summary>
    /// Invokes OpenGL calls.
    /// </summary>
    public class SilkInvoker : IGLInvoker
    {
        private static GL? gl;

        /// <summary>
        /// Initializes a new instance of the <see cref="SilkInvoker"/> class.
        /// </summary>
        /// <param name="window">The window used for rendering.</param>
        public SilkInvoker(IWindow window)
        {
            if (gl is null)
                gl = GL.GetApi(window);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SilkInvoker"/> class.
        /// </summary>
        public SilkInvoker()
        {
            if (gl is null)
                gl = GL.GetApi(RaptorWindow.WindowInstance);
        }

        public unsafe void DebugCallback(DebugProc debugCallback) => gl.DebugMessageCallback(debugCallback, IntPtr.Zero.ToPointer());

        public void ActiveTexture(TextureUnit texture) => gl.ActiveTexture(texture);

        public void AttachShader(uint program, uint shader) => gl.AttachShader(program, shader);

        public void BindBuffer(BufferTargetARB target, uint buffer) => gl.BindBuffer(target, buffer);

        public void BindTexture(TextureTarget target, uint texture) => gl.BindTexture(target, texture);

        public void BindVertexArray(uint array) => gl.BindVertexArray(array);

        public void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor) => gl.BlendFunc(sfactor, dfactor);

        public unsafe void BufferData(BufferTargetARB target, uint size, BufferUsageARB usage) => gl.BufferData(target, size, null, usage);

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message only used inside of method.")]
        public unsafe void BufferData(BufferTargetARB target, uint size, uint[] data, BufferUsageARB usage)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), "The param must not be null.");

            fixed (uint* dataPtr = &data[0])
            {
                gl.BufferData(target, size, dataPtr, usage);
            }
        }

        public unsafe void BufferSubData<T>(BufferTargetARB target, int offset, uint size, T data)
            where T : unmanaged => gl.BufferSubData(target, offset, size, &data);

        public void Clear() => gl.Clear((uint)GLEnum.ColorBufferBit);

        public void ClearColor(float red, float green, float blue, float alpha) => gl.ClearColor(red, green, blue, alpha);

        public void ViewPort(Size size) => gl.Viewport(size);

        public void CompileShader(uint shader) => gl.CompileShader(shader);

        public uint CreateProgram() => gl.CreateProgram();

        public uint CreateShader(ShaderType type) => gl.CreateShader(type);

        public void DeleteBuffer(uint buffers) => gl.DeleteBuffer(buffers);

        public void DeleteProgram(uint program) => gl.DeleteProgram(program);

        public void DeleteShader(uint shader) => gl.DeleteShader(shader);

        public void DeleteTexture(uint textures) => gl.DeleteTexture(textures);

        public void DeleteVertexArray(uint arrays) => gl.DeleteVertexArray(arrays);

        public void DetachShader(uint program, uint shader) => gl.DetachShader(program, shader);

        public unsafe void DrawElements(PrimitiveType mode, uint count, DrawElementsType type, void* indices) => gl.DrawElements(mode, count, type, indices);

        public void Enable(EnableCap cap) => gl.Enable(cap);

        public void EnableVertexArrayAttrib(uint vaobj, uint index) => gl.EnableVertexArrayAttrib(vaobj, index);

        public uint GenBuffer() => gl.GenBuffer();

        public uint GenTexture() => gl.GenTexture();

        public uint GenVertexArray() => gl.GenVertexArray();

        public unsafe void GetProgram(uint program, ProgramPropertyARB pname, out int programParams)
        {
            fixed (int* progParams = &programParams)
            {
                gl.GetProgram(program, pname, progParams);
            }
        }

        public string GetProgramInfoLog(uint program) => gl.GetProgramInfoLog(program);

        public void GetShader(uint shader, ShaderParameterName pname, out int shaderParams) => gl.GetShader(shader, pname, out shaderParams);

        public string GetShaderInfoLog(uint shader) => gl.GetShaderInfoLog(shader);

        public int GetUniformLocation(uint program, string name) => gl.GetUniformLocation(program, name);

        public void LinkProgram(uint program) => gl.LinkProgram(program);

        public bool LinkProgramSuccess(uint program)
        {
            gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var statusCode);

            return statusCode >= 1;
        }

        public void ObjectLabel(ObjectIdentifier identifier, uint name, uint length, string label) => gl.ObjectLabel(identifier, name, length, label);

        public bool ShaderCompileSuccess(uint shaderID)
        {
            gl.GetShader(shaderID, ShaderParameterName.CompileStatus, out var statusCode);

            return statusCode >= 1;
        }

        public void ShaderSource(uint shader, string sourceCode) => gl.ShaderSource(shader, sourceCode);

        public unsafe void TexImage2D(TextureTarget target, int level, PixelFormat internalformat, uint width, uint height, int border, PixelFormat format, PixelType type, byte[] pixels)
        {
            fixed (byte* pixelPtr = pixels)
            {
                gl.TexImage2D(target, level, (int)internalformat, width, height, border, format, type, pixelPtr);
            }
        }

        public void TexParameter(TextureTarget target, TextureParameterName pname, int param) => gl.TexParameter(target, pname, param);

        public unsafe void UniformMatrix4x4(int location, bool transpose, Matrix4x4 matrix) => gl.UniformMatrix4(location, 1, transpose, (float*)&matrix);

        public void UseProgram(uint program) => gl.UseProgram(program);

        public unsafe void VertexAttribPointer(uint index, int size, VertexAttribPointerType type, bool normalized, uint stride, uint offset) => gl.VertexAttribPointer(index, size, type, normalized, stride, new IntPtr(offset).ToPointer());
    }
}
