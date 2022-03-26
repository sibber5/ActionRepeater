using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace PathWindows;

public sealed class PathWindow : IDisposable
{
    private Thread? _thread;

    private PathForm? _pathForm;

    private bool _disposed;

    public PathWindow(Point[] points)
    {
        byte[] types = new byte[points.Length];
        types[0] = (byte)PathPointType.Start;
        Array.Fill(types, (byte)PathPointType.Line, 1, types.Length - 1);

        _thread = new Thread(() =>
        {
            _pathForm = new(new Pen(Color.Red, 4f), new GraphicsPath(points, types));
            Application.Run(_pathForm);
        });
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
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

    private class PathForm : Form
    {
        private Pen? _pen;
        private GraphicsPath? _path;

        public PathForm(Pen pen, GraphicsPath path)
        {
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;

            TransparencyKey = Color.FromArgb(254, 254, 254);
            BackColor = TransparencyKey;

            _pen = pen;
            _path = path;

            Paint += OnPaint;
            FormClosed += OnClosed;
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

            Debug.WriteLine("Disposed path form.");
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawPath(_pen!, _path!);
        }

        protected override void WndProc(ref Message m)
        {
            // WM_NCHITTEST
            if (m.Msg == 0x0084)
            {
                m.Result = new IntPtr(-1);
                return;
            }

            base.WndProc(ref m);
        }
    }
}
