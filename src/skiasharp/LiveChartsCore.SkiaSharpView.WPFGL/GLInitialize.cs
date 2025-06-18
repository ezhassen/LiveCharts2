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
using System.Windows.Controls;
using System.Windows.Media;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;
using SkiaSharpEzz.Views.WPFGL;

namespace LiveChartsCore.SkiaSharpView.WPF;

/// <summary>
/// Initialize to use GPU Render control (Using GL
/// </summary>
public static class GLInitialize
{
    /// <summary>
    /// Initialize to use GPU Render control
    /// </summary>
    public static void InitializeRenderGPU() => WPFSettings.FuncInitializeRenderFrameworkElement = GetGPURenderFrameworkElement;
    private static FrameworkElement GetGPURenderFrameworkElement(CoreMotionCanvas CanvasCore, MotionCanvas motionCanvas)
    {

        // workaround #250115
#if NET6_0_OR_GREATER
        static ResolutionHelper GetPixelDensity(MotionCanvas motionCanvas)
        {
            var presentationSource = PresentationSource.FromVisual(motionCanvas);
            if (presentationSource is null) return new(1f, 1f);
            var compositionTarget = presentationSource.CompositionTarget;
            if (compositionTarget is null) return new(1f, 1f);

            var matrix = compositionTarget.TransformToDevice;
            return new((float)matrix.M11, (float)matrix.M22);
        }

        var _skiaGlElement = new SKGLElement();
        _skiaGlElement = new SKGLElement();
        _skiaGlElement.PaintSurface += (sender, args) =>
        {
            var density = GetPixelDensity(motionCanvas);
            args.Surface.Canvas.Scale(density.dpix, density.dpiy);

            var c = (((Control)motionCanvas.Parent).Background is not SolidColorBrush bg)
                ? Colors.White
                : bg.Color;

            CanvasCore.DrawFrame(
                new SkiaSharpDrawingContext(CanvasCore, args.Info, args.Surface, args.Surface.Canvas)
                {
                    Background = new SKColor(c.R, c.G, c.B)
                });
        };
        return _skiaGlElement;
#else
        throw new PlatformNotSupportedException(
                        "GPU rendering is only supported in .NET 6.0 or greater, " +
                        "because https://github.com/mono/SkiaSharp/issues/3111 needs to be fixed.");
#endif


    }


}
