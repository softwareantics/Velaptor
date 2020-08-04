﻿// <copyright file="ISound.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Raptor.Audio
{
    using System;

    /// <summary>
    /// A single sound that can be played, paused etc.
    /// </summary>
    public interface ISound : IDisposable
    {
        /// <summary>
        /// Gets the name of the sound.
        /// </summary>
        string ContentName { get; }

        /// <summary>
        /// Gets or sets the volume of the sound.
        /// </summary>
        /// <remarks>
        ///     The only valid value accepted is 0-100. If a value outside of
        ///     that range is used, it will be set withing that range.
        /// </remarks>
        public float Volume { get; set; }

        /// <summary>
        /// Gets the current time position of the sound in milliseconds.
        /// </summary>
        public float TimePositionMilliseconds { get; }

        /// <summary>
        /// Gets the current time position of the sound in seconds.
        /// </summary>
        public float TimePositionSeconds { get; }

        /// <summary>
        /// Gets the current time position of the sound in minutes.
        /// </summary>
        float TimePositionMinutes { get; }

        /// <summary>
        /// Gets the current time position of the sound.
        /// </summary>
        TimeSpan TimePosition { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the sound loops back to the beginning once the end is reached.
        /// </summary>
        public bool IsLooping { get; set; }

        /// <summary>
        /// Plays the sound.
        /// </summary>
        public void PlaySound();

        /// <summary>
        /// Pauses the sound.
        /// </summary>
        public void PauseSound();

        /// <summary>
        /// Stops the sound.
        /// </summary>
        /// <remarks>This will set the time position back to the beginning.</remarks>
        public void StopSound();

        /// <summary>
        /// Resets the sound.
        /// </summary>
        /// <remarks>
        ///     This will stop the sound and set the time position back to the beginning.
        /// </remarks>
        public void Reset();

        /// <summary>
        /// Sets the time position of the sound to the given value.
        /// </summary>
        /// <param name="seconds">The time position in seconds of where to set the sound.</param>
        public void SetTimePosition(float seconds);
    }
}
