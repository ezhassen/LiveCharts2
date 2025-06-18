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

using System.Windows.Forms;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharpEzz.Views.Desktop;

namespace LiveChartsCore.SkiaSharpView.WinForms;

/// <summary>
/// Initialize to use GPU Render control (Using GL)
/// </summary>
public static class GLInitialize
{
    /// <summary>
    /// Initialize to use GPU Render control
    /// </summary>
    public static void InitializeRenderGPU() => WinFormsSettings.FuncInitializeRenderControl = GetGPURenderControl;

    private static Control GetGPURenderControl(CoreMotionCanvas CanvasCore)
    {
        var _skControl = new SKGLControl();
        _skControl.PaintSurface += (ss, ee) =>
        {
            SkiaSharp.SKColor backColor;

            if (_skControl.Parent is not null)
            {
                backColor = _skControl.Parent.Parent is not null
                    ? new SkiaSharp.SKColor(_skControl.Parent.Parent!.BackColor.R, _skControl.Parent.Parent.BackColor.G, _skControl.Parent.Parent.BackColor.B)
                    : new SkiaSharp.SKColor(_skControl.Parent!.BackColor.R, _skControl.Parent.BackColor.G, _skControl.Parent.BackColor.B);
            }
            else
            {
                backColor = new SkiaSharp.SKColor(250, 250, 250);
            }
            CanvasCore.DrawFrame(
             new SkiaSharpDrawingContext(CanvasCore, ee.Info, ee.Surface, ee.Surface.Canvas)
             {
                 Background = backColor
             });
        };

        return _skControl;
    }
}
