using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace PathWindows;

// TODO: switch to win2d when transparent backgronds are implemented
// transparent window bg: https://github.com/microsoft/microsoft-ui-xaml/issues/1247
// win2d: https://www.nuget.org/packages/Microsoft.Graphics.Win2D
public sealed class PathWindow : IDisposable
{
    private Thread? _thread;

    private PathForm? _pathForm;

    private bool _disposed;

    public PathWindow(Point[]? points = null)
    {
        if (points is null)
        {
            _thread = new Thread(() =>
            {
                _pathForm = new(new Pen(Color.Red, 3), new GraphicsPath());
                Application.Run(_pathForm);
            });
        }
        else
        {
            byte[] types = new byte[points.Length];
            types[0] = (byte)PathPointType.Start;
            Array.Fill(types, (byte)PathPointType.Line, 1, types.Length - 1);

            _thread = new Thread(() =>
            {
                _pathForm = new(new Pen(Color.Red, 3), new GraphicsPath(points, types));
                Application.Run(_pathForm);
            });
        }

        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    public void AddLineToPath(Point prevPoint, Point newPoint)
    {
        _pathForm?.AddLine(prevPoint, newPoint);
    }

    public void ClearPath()
    {
        _pathForm?.ResetPath();
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        Debug.Assert(_pathForm is not null, "Path Form is null.");
        Debug.Assert(_thread is not null, "Thread is null.");
        Debug.Assert(_thread.ThreadState == System.Threading.ThreadState.Running, "Thread is not running.");

        if (disposing)
        {
            _pathForm!.Invoke(() => Application.Exit());
            _pathForm = null;
        }

        _thread.Join();
        _thread = null;

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private sealed class PathForm : Form
    {
        private Pen? _pen;
        private GraphicsPath? _path;

        private readonly ManualResetEventSlim _addLineMre = new(true);

        public PathForm(Pen pen, GraphicsPath path)
        {
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            DoubleBuffered = true;

            TransparencyKey = Color.FromArgb(254, 254, 254);
            BackColor = TransparencyKey;

            _pen = pen;
            _path = path;

            Paint += OnPaint;
            FormClosed += OnClosed;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                //                       WS_EX_COMPOSITED | WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_TRANSPARENT
                cp.ExStyle = cp.ExStyle | 0x02000000 | 0x00080000 | 0x08000000 | 0x00000008 | 0x00000020;

                return cp;
            }
        }

        public void AddLine(Point prevPoint, Point newPoint)
        {
            if (_path is null) return;

            _addLineMre.Wait();

            _path.AddLine(prevPoint, newPoint);
            Invalidate();
        }

        public void ResetPath()
        {
            _path!.Reset();
            Invalidate();
        }

        private void OnClosed(object? sender, FormClosedEventArgs e)
        {
            Paint -= OnPaint;
            FormClosed -= OnClosed;

            Dispose();

            _pen!.Dispose();
            _pen = null;

            _path!.Dispose();
            _path = null;

            _addLineMre.Dispose();

            Debug.WriteLine("Disposed path form.");
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            _addLineMre.Reset();

            //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawPath(_pen!, _path!);

            _addLineMre.Set();
        }
    }
}
