﻿// <copyright file="EnumOutOfRangeException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Exceptions;

using System;
using System.Runtime.Serialization;
using System.Security;
using Graphics;

/// <summary>
/// Thrown when an invalid <see cref="RenderEffects"/> value is used.
/// </summary>
/// <typeparam name="T">The type of enumeration.</typeparam>
[Serializable]
public sealed class EnumOutOfRangeException<T> : Exception
    where T : Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOutOfRangeException{T}"/> class.
    /// </summary>
    public EnumOutOfRangeException()
        : base($"The value of the enum '{typeof(T).Name}' is invalid and out of range.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOutOfRangeException{T}"/> class.
    /// </summary>
    /// <param name="className">The name of the class where the exception occurred.</param>
    /// <param name="methodName">The name of the method where the exception occured.</param>
    public EnumOutOfRangeException(string className, string methodName)
        : base($"The value of the enum '{typeof(T).Name}' used in the class '{className}' and method '{methodName}' is invalid and out of range.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOutOfRangeException{T}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EnumOutOfRangeException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOutOfRangeException{T}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    ///     The <see cref="Exception"/> instance that caused the current exception.
    /// </param>
    public EnumOutOfRangeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOutOfRangeException{T}"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    private EnumOutOfRangeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
