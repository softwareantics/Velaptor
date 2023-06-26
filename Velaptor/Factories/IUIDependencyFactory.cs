// <copyright file="IUIDependencyFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Factories;

using Carbonate.UniDirectional;
using ReactableData;
using UI;

internal interface IUIDependencyFactory
{
    public ITextSelection CreateTextSelection(IPushReactable<TextBoxStateData> textBoxDataReactable);
    public ITextCursor CreateTextCursor(IPushReactable<TextBoxStateData> textBoxDataReactable);
}
