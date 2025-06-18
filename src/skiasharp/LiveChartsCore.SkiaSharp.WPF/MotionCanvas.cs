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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore.Motion;

namespace LiveChartsCore.SkiaSharpView.WPF;

/// <summary>
/// Defines the motion canvas control for WPF, <see cref="CoreMotionCanvas"/>.
/// </summary>
/// <seealso cref="Control" />
public class MotionCanvas : UserControl
{

    private FrameworkElement? _renderFrameworkElement;

    private bool _isDrawingLoopRunning = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MotionCanvas"/> class.
    /// </summary>
    public MotionCanvas()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// Gets the canvas core.
    /// </summary>
    /// <value>
    /// The canvas core.
    /// </value>
    public CoreMotionCanvas CanvasCore { get; } = new();

    private void InitializeElement()
    {
        _renderFrameworkElement = WPFSettings.FuncInitializeRenderFrameworkElement(CanvasCore, this);
        Content = _renderFrameworkElement;
    }

    private ResolutionHelper GetPixelDensity()
    {
        var presentationSource = PresentationSource.FromVisual(this);
        if (presentationSource is null) return new(1f, 1f);
        var compositionTarget = presentationSource.CompositionTarget;
        if (compositionTarget is null) return new(1f, 1f);

        var matrix = compositionTarget.TransformToDevice;
        return new((float)matrix.M11, (float)matrix.M22);
    }

    private void OnCanvasCoreInvalidated(CoreMotionCanvas sender) =>
        RunDrawingLoop();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeElement();
        CanvasCore.Invalidated += OnCanvasCoreInvalidated;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CanvasCore.Invalidated -= OnCanvasCoreInvalidated;
        CanvasCore.Dispose();
    }

    private async void RunDrawingLoop()
    {
        if (_isDrawingLoopRunning) return;
        _isDrawingLoopRunning = true;

        var ts = TimeSpan.FromSeconds(1 / LiveCharts.MaxFps);

        while (!CanvasCore.IsValid)
        {
            _renderFrameworkElement?.InvalidateVisual();
            await Task.Delay(ts);
        }

        _isDrawingLoopRunning = false;
    }
}
