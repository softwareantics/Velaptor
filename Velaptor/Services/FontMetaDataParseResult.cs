// <copyright file="FontMetaDataParseResult.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Services;

/// <summary>
/// Holds information after the result of parsing font metadata from font file paths.
/// using the <see cref="FontMetaDataParser"/>.
/// </summary>
internal readonly struct FontMetaDataParseResult
{
    /// <summary>
    /// Gets a value indicating whether or not a string contains metadata.
    /// </summary>
    public readonly bool ContainsMetaData;

    /// <summary>
    /// Gets a value indicating whether or not the metadata in the string is valid.
    /// </summary>
    public readonly bool IsValid;

    /// <summary>
    /// Gets the data before the metadata section.
    /// </summary>
    public readonly string MetaDataPrefix;

    /// <summary>
    /// Gets the metadata string.
    /// </summary>
    public readonly string MetaData;

    /// <summary>
    /// Gets the font size that was embedded in the metadata.
    /// </summary>
    public readonly uint FontSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="FontMetaDataParseResult"/> struct.
    /// </summary>
    /// <param name="containsMetaData">Value indicating whether or not the string contains metadata.</param>
    /// <param name="isValid">Value indicating whether or not the metadata is valid.</param>
    /// <param name="metaData">The metadata in a string.</param>
    /// <param name="metaDataPrefix">The data before the metadata section.</param>
    /// <param name="fontSize">The size of the font embedded in the metadata.</param>
    public FontMetaDataParseResult(bool containsMetaData, bool isValid, string metaDataPrefix, string metaData, uint fontSize)
    {
        this.ContainsMetaData = containsMetaData;
        this.IsValid = isValid;
        this.MetaDataPrefix = metaDataPrefix;
        this.MetaData = metaData;
        this.FontSize = fontSize;
    }
}
