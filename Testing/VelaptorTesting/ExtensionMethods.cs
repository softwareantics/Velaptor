﻿// <copyright file="ExtensionMethods.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTesting;

using System;
using System.Drawing;
using System.Linq;
using Velaptor.Graphics;

public static class ExtensionMethods
{
    public static CornerRadius IncreaseTopLeft(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft + amount, radius.BottomLeft, radius.BottomRight, radius.TopRight);
    }

    public static CornerRadius DecreaseTopLeft(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft - amount, radius.BottomLeft, radius.BottomRight, radius.TopRight);
    }

    public static CornerRadius IncreaseBottomLeft(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft, radius.BottomLeft + amount, radius.BottomRight, radius.TopRight);
    }

    public static CornerRadius DecreaseBottomLeft(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft, radius.BottomLeft - amount, radius.BottomRight, radius.TopRight);
    }

    public static CornerRadius IncreaseBottomRight(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft, radius.BottomLeft, radius.BottomRight + amount, radius.TopRight);
    }

    public static CornerRadius DecreaseBottomRight(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft, radius.BottomLeft, radius.BottomRight - amount, radius.TopRight);
    }

    public static CornerRadius IncreaseTopRight(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft, radius.BottomLeft, radius.BottomRight, radius.TopRight + amount);
    }

    public static CornerRadius DecreaseTopRight(this CornerRadius radius, float amount)
    {
        return new CornerRadius(radius.TopLeft, radius.BottomLeft, radius.BottomRight, radius.TopRight - amount);
    }

    public static void IncreaseWidth(ref this RectShape rect, float amount) => rect.Width += amount;

    public static void DecreaseWidth(ref this RectShape rect, float amount) => rect.Width -= amount;

    public static void IncreaseHeight(ref this RectShape rect, float amount) => rect.Height += amount;

    public static void DecreaseHeight(ref this RectShape rect, float amount) => rect.Height -= amount;

    public static void IncreaseDiameter(ref this CircleShape circle, float amount) => circle.Diameter += amount;

    public static void DecreaseDiameter(ref this CircleShape circle, float amount) => circle.Diameter -= amount;

    public static void SwapFillColor(ref this RectShape rect, Color[] colors, Action<string> callback)
    {
        if (colors.Length < 3)
        {
            return;
        }

        var clrStr = "ERROR";

        if (rect.Color == colors[0])
        {
            rect.Color = colors[1];
            clrStr = "Green";
        }
        else if (rect.Color == colors[1])
        {
            rect.Color = colors[2];
            clrStr = "Blue";
        }
        else if (rect.Color == colors[2])
        {
            rect.Color = colors[0];
            clrStr = "Red";
        }

        callback(clrStr);
    }

    public static void SwapFillColor(ref this CircleShape circle, Color[] colors, Action<string> callback)
    {
        if (colors.Length < 3)
        {
            return;
        }

        var clrStr = "ERROR";

        if (circle.Color == colors[0])
        {
            circle.Color = colors[1];
            clrStr = "Green";
        }
        else if (circle.Color == colors[1])
        {
            circle.Color = colors[2];
            clrStr = "Blue";
        }
        else if (circle.Color == colors[2])
        {
            circle.Color = colors[0];
            clrStr = "Red";
        }

        callback(clrStr);
    }

    public static void SwapGradStartColor(ref this RectShape rect, Color[] colors, Action<string> callback)
    {
        if (colors.Length < 3)
        {
            return;
        }

        var clrStr = "ERROR";

        if (rect.GradientStart == colors[0])
        {
            rect.GradientStart = colors[1];
            clrStr = "Green";
        }
        else if (rect.GradientStart == colors[1])
        {
            rect.GradientStart = colors[2];
            clrStr = "Blue";
        }
        else if (rect.GradientStart == colors[2])
        {
            rect.GradientStart = colors[0];
            clrStr = "Red";
        }

        callback(clrStr);
    }

    public static void SwapGradStartColor(ref this CircleShape circle, Color[] colors, Action<string> callback)
    {
        if (colors.Length < 3)
        {
            return;
        }

        var clrStr = "ERROR";

        if (circle.GradientStart == colors[0])
        {
            circle.GradientStart = colors[1];
            clrStr = "Green";
        }
        else if (circle.GradientStart == colors[1])
        {
            circle.GradientStart = colors[2];
            clrStr = "Blue";
        }
        else if (circle.GradientStart == colors[2])
        {
            circle.GradientStart = colors[0];
            clrStr = "Red";
        }

        callback(clrStr);
    }

    public static void SwapGradStopColor(ref this RectShape rect, Color[] colors, Action<string> callback)
    {
        if (colors.Length < 3)
        {
            return;
        }

        var clrStr = "ERROR";

        if (rect.GradientStop == colors[0])
        {
            rect.GradientStop = colors[1];
            clrStr = "Green";
        }
        else if (rect.GradientStop == colors[1])
        {
            rect.GradientStop = colors[2];
            clrStr = "Blue";
        }
        else if (rect.GradientStop == colors[2])
        {
            rect.GradientStop = colors[0];
            clrStr = "Red";
        }

        callback(clrStr);
    }

    public static void SwapGradStopColor(ref this CircleShape circle, Color[] colors, Action<string> callback)
    {
        if (colors.Length < 3)
        {
            return;
        }

        var clrStr = "ERROR";

        if (circle.GradientStop == colors[0])
        {
            circle.GradientStop = colors[1];
            clrStr = "Green";
        }
        else if (circle.GradientStop == colors[1])
        {
            circle.GradientStop = colors[2];
            clrStr = "Blue";
        }
        else if (circle.GradientStop == colors[2])
        {
            circle.GradientStop = colors[0];
            clrStr = "Red";
        }

        callback(clrStr);
    }

    public static ColorGradient SwapGradient(this ColorGradient gradient)
    {
        return gradient switch
        {
            ColorGradient.None => ColorGradient.Horizontal,
            ColorGradient.Horizontal => ColorGradient.Vertical,
            ColorGradient.Vertical => ColorGradient.None,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static T Next<T>(this T enumValue)
        where T : Enum
    {
        T[] enumValues = (T[])Enum.GetValues(typeof(T));
        var currentIndex = Array.IndexOf(enumValues, enumValue);
        var nextIndex = (currentIndex + 1) % enumValues.Length;

        return enumValues[nextIndex];
    }

    public static T Previous<T>(this T enumValue)
        where T : Enum
    {
        T[] enumValues = (T[])Enum.GetValues(typeof(T));
        var currentIndex = Array.IndexOf(enumValues, enumValue);
        var nextIndex = (currentIndex - 1) % enumValues.Length;
        nextIndex = nextIndex < 0
            ? Array.IndexOf(enumValues, enumValues[^1])
            : nextIndex;

        return enumValues[nextIndex];
    }

    public static Color IncreaseRedBy(this Color clr, int amount)
    {
        var newValue = clr.R + amount;
        newValue = newValue > 255 ? 255  : newValue;

        return Color.FromArgb(clr.A, newValue, clr.G, clr.B);
    }

    public static Color DecreaseRedBy(this Color clr, int amount)
    {
        var newValue = clr.R - amount;
        newValue = newValue < 0 ? 0 : newValue;

        return Color.FromArgb(clr.A, newValue, clr.G, clr.B);
    }

    public static Color IncreaseGreenBy(this Color clr, int amount)
    {
        var newValue = clr.G + amount;
        newValue = newValue > 255 ? 255  : newValue;

        return Color.FromArgb(clr.A, clr.R, newValue, clr.B);
    }

    public static Color DecreaseGreenBy(this Color clr, int amount)
    {
        var newValue = clr.G - amount;
        newValue = newValue < 0 ? 0 : newValue;

        return Color.FromArgb(clr.A, clr.R, newValue, clr.B);
    }

    public static Color IncreaseBlueBy(this Color clr, int amount)
    {
        var newValue = clr.B + amount;
        newValue = newValue > 255 ? 255  : newValue;

        return Color.FromArgb(clr.A, clr.R, clr.G, newValue);
    }

    public static Color DecreaseBlueBy(this Color clr, int amount)
    {
        var newValue = clr.B - amount;
        newValue = newValue < 0 ? 0 : newValue;

        return Color.FromArgb(clr.A, clr.R, clr.G, newValue);
    }
}
