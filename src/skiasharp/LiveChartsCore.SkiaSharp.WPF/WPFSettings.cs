// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Windows;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharpEzz.Views.WPF;

namespace LiveChartsCore.SkiaSharpView.WPF;
/// <summary>
/// WPF Settings for rendering
/// </summary>
public static class WPFSettings
{

    private static Func<CoreMotionCanvas, MotionCanvas, FrameworkElement>? s_funcInitializeRenderFrameworkElement;
    /// <summary>
    /// To Get render FrameworkElement
    /// </summary>
    public static Func<CoreMotionCanvas, MotionCanvas, FrameworkElement> FuncInitializeRenderFrameworkElement
    {
        get
        {
            s_funcInitializeRenderFrameworkElement ??= GetNativeRenderFrameworkElement;
            return s_funcInitializeRenderFrameworkElement;
        }

        set => s_funcInitializeRenderFrameworkElement = value;
    }

    private static SKElement GetNativeRenderFrameworkElement(CoreMotionCanvas CanvasCore, MotionCanvas motionCanvas)
    {
        var skElement = new SKElement();
        skElement.PaintSurface += (sender, args) =>
        {
            var density = GetPixelDensity(motionCanvas);
            args.Surface.Canvas.Scale(density.dpix, density.dpiy);
            CanvasCore.DrawFrame(
                new SkiaSharpDrawingContext(CanvasCore, args.Info, args.Surface, args.Surface.Canvas));
        };
        return skElement;
    }

    private static ResolutionHelper GetPixelDensity(MotionCanvas motionCanvas)
    {
        var presentationSource = PresentationSource.FromVisual(motionCanvas);
        if (presentationSource is null) return new(1f, 1f);
        var compositionTarget = presentationSource.CompositionTarget;
        if (compositionTarget is null) return new(1f, 1f);

        var matrix = compositionTarget.TransformToDevice;
        return new((float)matrix.M11, (float)matrix.M22);
    }
}
