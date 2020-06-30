// <copyright file="VertexDataAnalyzer.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Raptor.OpenGL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Silk.NET.OpenGL;

    internal static class VertexDataAnalyzer
    {
        private static readonly Dictionary<Type, uint> ValidTypeSizes = new Dictionary<Type, uint>()
        {
            // In order from least to greatest bytes
            { typeof(byte), sizeof(byte) },
            { typeof(sbyte), sizeof(sbyte) },
            { typeof(short), sizeof(short) },
            { typeof(ushort), sizeof(ushort) },
            { typeof(int), sizeof(int) },
            { typeof(uint), sizeof(uint) },
            { typeof(float), sizeof(float) },
            { typeof(long), sizeof(long) },
            { typeof(ulong), sizeof(ulong) },
            { typeof(double), sizeof(double) },
            { typeof(Vector2), sizeof(float) * 2 },
            { typeof(Vector3), sizeof(float) * 3 },
            { typeof(Vector4), sizeof(float) * 4 },
            { typeof(Matrix4x4), sizeof(float) * 16 },
        };

        private static readonly Dictionary<Type, uint> TotalItemsForTypes = new Dictionary<Type, uint>()
        {
            // In order from least to greatest bytes
            { typeof(byte), 1 },
            { typeof(sbyte), 1 },
            { typeof(short), 1 },
            { typeof(ushort), 1 },
            { typeof(int), 1 },
            { typeof(uint), 1 },
            { typeof(float), 1 },
            { typeof(long), 1 },
            { typeof(ulong), 1 },
            { typeof(double), 1 },
            { typeof(Vector2), 2 },
            { typeof(Vector3), 3 },
            { typeof(Vector4), 4 },
            { typeof(Matrix4x4), 16 },
        };

        // TODO: Need to find out the rest of the mappings
        private static readonly Dictionary<Type, VertexAttribPointerType> PointerTypeMappings = new Dictionary<Type, VertexAttribPointerType>()
        {
            { typeof(float), VertexAttribPointerType.Float },
            { typeof(Vector2), VertexAttribPointerType.Float },
            { typeof(Vector3), VertexAttribPointerType.Float },
            { typeof(Vector4), VertexAttribPointerType.Float },
        };

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception text not used outside of method.")]
        public static uint GetTotalBytesForStruct(Type structType)
        {
            if (structType is null)
                throw new ArgumentNullException(nameof(structType), "The argument must not be null");

            if (!IsStruct(structType))
                throw new Exception($"The given '{nameof(structType)}' must be a struct.");

            var publicFields = structType.GetFields();
            var result = 0u;

            // If any types are not of the valid type list, throw an exception
            foreach (var field in publicFields)
            {
                if (!ValidTypeSizes.ContainsKey(field.FieldType))
                    throw new Exception($"The type '{field.FieldType}' is not allowed in vertex buffer data structure.");

                result += ValidTypeSizes[field.FieldType];
            }

            return result;
        }

        public static uint GetTypeByteSize(Type type) => ValidTypeSizes[type];

        public static uint TotalItemsForType(Type type) => TotalItemsForTypes[type];

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception text not used outside of method.")]
        public static uint GetVertexSubDataOffset(Type structType, string subDataName)
        {
            if (structType is null)
                throw new ArgumentNullException(nameof(structType), "The argument must not be null");

            if (!IsStruct(structType))
                throw new Exception($"The given '{nameof(structType)}' must be a struct.");

            var publicFields = structType.GetFields();
            var result = 0u;

            // If any types are not of the valid type list, throw an exception
            foreach (var field in publicFields)
            {
                if (!ValidTypeSizes.ContainsKey(field.FieldType))
                    throw new Exception($"The type '{field.FieldType}' is not allowed in vertex buffer data structure.");

                // If the type is not the field of the given name.
                // Get all of the fields sequentially up unto the sub data name field
                if (field.Name != subDataName)
                {
                    result += ValidTypeSizes[field.FieldType];
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public static VertexAttribPointerType GetVertexPointerType(Type type) => PointerTypeMappings[type];

        private static bool IsStruct(Type type) => type.IsValueType && !type.IsEnum;
    }
}
