// <copyright file="RectBatchingService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Carbonate;
using Exceptions;
using Guards;
using OpenGL;
using ReactableData;

/// <summary>
/// Manages the process of batching the rendering of rectangles.
/// </summary>
internal sealed class RectBatchingService : IBatchingService<RectBatchItem>
{
    private readonly IDisposable unsubscriber;
    private readonly IPushReactable reactable;
    private RectBatchItem[] batchItems = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="RectBatchingService"/> class.
    /// </summary>
    /// <param name="reactable">Sends and receives push notifications.</param>
    public RectBatchingService(IPushReactable reactable)
    {
        EnsureThat.ParamIsNotNull(reactable);

        this.reactable = reactable;

        var batchSizeName = this.GetExecutionMemberName(nameof(NotificationIds.BatchSizeSetId));
        this.unsubscriber = reactable.Subscribe(new ReceiveReactor(
            eventId: NotificationIds.BatchSizeSetId,
            name: batchSizeName,
            onReceiveMsg: msg =>
            {
                var batchSize = msg.GetData<BatchSizeData>()?.BatchSize;

                if (batchSize is null)
                {
                    throw new PushNotificationException(
                        $"{nameof(RectBatchingService)}.Constructor()",
                        NotificationIds.BatchSizeSetId);
                }

                var items = new List<RectBatchItem>();

                for (var i = 0u; i < batchSize; i++)
                {
                    items.Add(default);
                }

                this.batchItems = items.ToArray();
            },
            onUnsubscribe: () => this.unsubscriber?.Dispose()));
    }

    /// <inheritdoc/>
    public ReadOnlyCollection<RectBatchItem> BatchItems
    {
        get => new (this.batchItems);
        set => this.batchItems = value.ToArray();
    }

    /// <summary>
    /// Adds the given <paramref name="rect"/> to the batch.
    /// </summary>
    /// <param name="rect">The item to be added.</param>
    public void Add(in RectBatchItem rect)
    {
        var batchIsFull = this.batchItems.All(i => i.IsEmpty() is false);

        if (batchIsFull)
        {
            this.reactable.Push(NotificationIds.RenderRectsId);
        }

        var emptyItemIndex = this.batchItems.IndexOf(i => i.IsEmpty());

        if (emptyItemIndex == -1)
        {
            return;
        }

        this.batchItems[emptyItemIndex] = rect;
    }

    /// <summary>
    /// Empties the entire batch.
    /// </summary>
    /// <remarks>
    /// An empty batch are items that are set NOT set to render and have all values set to default.
    /// </remarks>
    public void EmptyBatch()
    {
        for (var i = 0u; i < this.batchItems.Length; i++)
        {
            if (this.batchItems[i].IsEmpty())
            {
                continue;
            }

            this.batchItems[i] = default;
        }
    }
}
