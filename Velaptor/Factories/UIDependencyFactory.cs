// <copyright file="UIDependencyFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Factories;

using Carbonate.UniDirectional;
using ReactableData;
using UI;

internal class UIDependencyFactory : IUIDependencyFactory
{
    public ITextSelection CreateTextSelection(IPushReactable<TextBoxStateData> textBoxDataReactable) =>
        new TextSelection(textBoxDataReactable);

    public ITextCursor CreateTextCursor(IPushReactable<TextBoxStateData> textBoxDataReactable) =>
        new TextCursor(textBoxDataReactable);
}
