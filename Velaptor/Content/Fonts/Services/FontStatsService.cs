﻿// <copyright file="FontStatsService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Content.Fonts.Services;

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Guards;

/// <inheritdoc cref="IFontStatsService"/>
internal sealed class FontStatsService : IFontStatsService
{
    private const string FontFileExtension = ".ttf";
    private readonly Dictionary<string, FontStats> contentFontStatsCache = new ();
    private readonly Dictionary<string, FontStats> systemFontStatsCache = new ();
    private readonly IFontService fontService;
    private readonly IContentPathResolver sysFontPathResolver;
    private readonly IContentPathResolver contentPathResolver;
    private readonly IDirectory directory;
    private readonly IPath path;

    /// <summary>
    /// Initializes a new instance of the <see cref="FontStatsService"/> class.
    /// </summary>
    /// <param name="fontService">Provides extensions/helpers to <c>FreeType</c> library functionality.</param>
    /// <param name="contentPathResolver">Resolves paths to the application's content directory.</param>
    /// <param name="sysFontPathResolver">Resolves paths to the systems font directory.</param>
    /// <param name="directory">Performs operations with directories.</param>
    /// <param name="path">Processes directory and file paths.</param>
    public FontStatsService(
        IFontService fontService,
        IContentPathResolver contentPathResolver,
        IContentPathResolver sysFontPathResolver,
        IDirectory directory,
        IPath path)
    {
        EnsureThat.ParamIsNotNull(fontService);
        EnsureThat.ParamIsNotNull(contentPathResolver);
        EnsureThat.ParamIsNotNull(sysFontPathResolver);
        EnsureThat.ParamIsNotNull(directory);
        EnsureThat.ParamIsNotNull(path);

        this.fontService = fontService;
        this.contentPathResolver = contentPathResolver;
        this.sysFontPathResolver = sysFontPathResolver;
        this.directory = directory;
        this.path = path;
    }

    /// <inheritdoc/>
    public FontStats[] GetContentStatsForFontFamily(string fontFamilyName)
    {
        // If any items already exist with the given font family, then it has already been added
        var foundFamilyItems = (from s in this.contentFontStatsCache.Values
            where s.FamilyName == fontFamilyName
            select s).ToArray();

        if (foundFamilyItems.Length > 0)
        {
            return foundFamilyItems;
        }

        var fontFiles = this.directory.GetFiles(this.contentPathResolver.ResolveDirPath(), $"*{FontFileExtension}");

        var results =
            (from filePath in fontFiles
                where this.fontService.GetFamilyName(filePath) == fontFamilyName
                select new FontStats
                {
                    FontFilePath = filePath,
                    FamilyName = fontFamilyName,
                    Style = this.fontService.GetFontStyle(filePath),
                    Source = GetFontSource(filePath),
                }).ToArray();

        foreach (var result in results)
        {
            this.contentFontStatsCache.Add(result.FontFilePath, result);
        }

        return (from s in this.contentFontStatsCache.Values
            where s.FamilyName == fontFamilyName
            select s).ToArray();
    }

    /// <inheritdoc/>
    public FontStats[] GetSystemStatsForFontFamily(string fontFamilyName)
    {
        // If any items already exist with the give font family, then it has already been added
        var foundFamilyItems = (from s in this.systemFontStatsCache.Values
            where s.FamilyName == fontFamilyName
            select s).ToArray();

        if (foundFamilyItems.Length > 0)
        {
            return foundFamilyItems;
        }

        var fontFiles = this.directory.GetFiles(this.sysFontPathResolver.ResolveDirPath(), $"*{FontFileExtension}");

        var results =
            (from filePath in fontFiles
                where this.fontService.GetFamilyName(filePath) == fontFamilyName
                select new FontStats
                {
                    FontFilePath = filePath,
                    FamilyName = fontFamilyName,
                    Style = this.fontService.GetFontStyle(filePath),
                    Source = GetFontSource(filePath),
                }).ToArray();

        foreach (var result in results)
        {
            this.systemFontStatsCache.Add(result.FontFilePath, result);
        }

        return (from s in this.systemFontStatsCache.Values
            where s.FamilyName == fontFamilyName
            select s).ToArray();
    }

    /// <summary>
    /// Returns the source of the font using the given <paramref name="fileOrDirPath"/>.
    /// </summary>
    /// <param name="fileOrDirPath">The directory or file path of the font.</param>
    /// <returns>The source of the font.</returns>
    private FontSource GetFontSource(string fileOrDirPath)
    {
        var contentFontDirPath = this.contentPathResolver.ResolveDirPath().ToLower();
        var sysFontDirPath = this.sysFontPathResolver.ResolveDirPath().ToLower();

        fileOrDirPath = (this.path.GetDirectoryName(fileOrDirPath) ?? string.Empty).ToLower();

        if (fileOrDirPath != contentFontDirPath && fileOrDirPath != sysFontDirPath)
        {
            return FontSource.Unknown;
        }

        return fileOrDirPath == contentFontDirPath
            ? FontSource.AppContent
            : FontSource.System;
    }
}
