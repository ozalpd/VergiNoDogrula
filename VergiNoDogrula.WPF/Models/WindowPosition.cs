using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json.Serialization;
using System.Windows;
using System.Xml.Serialization;

namespace VergiNoDogrula.WPF.Models
{
    [SupportedOSPlatform("windows")]
    internal class WindowPosition
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public bool IsOnPrimaryScreen { get; private set; }


        public void GetWindowPositions(Window window)
        {
            Top = window.Top;
            Left = window.Left;
            Height = window.Height;
            Width = window.Width;
        }

        public void SetWindowPositions(Window window)
        {
            double taskBarHeight = (SystemParameters.PrimaryScreenHeight - SystemParameters.MaximizedPrimaryScreenHeight) + 10;
            double maxWinHeight = SystemParameters.VirtualScreenHeight - taskBarHeight;
            double maxWinWidth = SystemParameters.VirtualScreenWidth;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenLeft = SystemParameters.VirtualScreenLeft;

            Rect windowRectangle = new Rect(Left, Top, Width, Height);
            var minVisible = new Size(10.0, 10.0);

            // Find the screen that contains most of the window, accounting for DPI
            var monitors = EnumerateMonitors();
            if (monitors.Count > 1)
            {
                foreach (var monitorInfo in monitors)
                {
                    var intersection = Rect.Intersect(windowRectangle, monitorInfo.WorkingArea);
                    if (intersection.Width >= minVisible.Width && intersection.Height >= minVisible.Height)
                    {
                        maxWinHeight = monitorInfo.WorkingArea.Height;
                        maxWinWidth = monitorInfo.WorkingArea.Width;
                        screenTop = monitorInfo.WorkingArea.Top;
                        screenLeft = monitorInfo.WorkingArea.Left;
                        IsOnPrimaryScreen = monitorInfo.IsPrimary;

                        break;
                    }
                }
            }

            if (Top < screenTop || Top + window.MinHeight >= screenTop + maxWinHeight || Height > maxWinHeight)
                Top = screenTop;

            if (Left < screenLeft || Left + window.MinWidth >= screenLeft + maxWinWidth || Width > maxWinWidth)
                Left = screenLeft;

            if (Height > maxWinHeight)
            {
                Top = screenTop;
                Height = maxWinHeight;
            }

            if (Width > maxWinWidth)
            {
                Left = screenLeft;
                Width = maxWinWidth;
            }

            if ((Left + Width) > screenLeft + maxWinWidth)
                Width = 0;

            if ((Top + Height) > screenTop + maxWinHeight)
                Height = 0;

            if (Width > 0)
                window.Width = Width;

            if (Height > 0)
                window.Height = Height;

            window.Top = Top;
            window.Left = Left;
        }

        /// <summary>
        /// Enumerates all monitors on the system with DPI-aware working areas.
        /// </summary>
        private static List<MonitorInfo> EnumerateMonitors()
        {
            var monitors = new List<MonitorInfo>();
            var allMonitors = new List<IntPtr>();

            NativeMethods.EnumDisplayMonitors(
                IntPtr.Zero,
                IntPtr.Zero,
                (hMonitor, hdcMonitor, ref lprcMonitor, dwData) =>
                {
                    allMonitors.Add(hMonitor);
                    return true;
                },
                IntPtr.Zero);

            foreach (var hMonitor in allMonitors)
            {
                var monitorInfo = new NativeMethods.MONITORINFOEX();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);

                if (NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    var workingAreaRect = monitorInfo.rcWork;
                    var bounds = new NativeMethods.RECT
                    {
                        left = monitorInfo.rcMonitor.left,
                        top = monitorInfo.rcMonitor.top,
                        right = monitorInfo.rcMonitor.right,
                        bottom = monitorInfo.rcMonitor.bottom
                    };

                    var dpiScale = GetScreenDpiScale(hMonitor, bounds);
                    
                    var workingArea = new Rect(
                        workingAreaRect.left / dpiScale.DpiScaleX,
                        workingAreaRect.top / dpiScale.DpiScaleY,
                        (workingAreaRect.right - workingAreaRect.left) / dpiScale.DpiScaleX,
                        (workingAreaRect.bottom - workingAreaRect.top) / dpiScale.DpiScaleY);

                    monitors.Add(new MonitorInfo
                    {
                        Handle = hMonitor,
                        WorkingArea = workingArea,
                        IsPrimary = (monitorInfo.dwFlags & NativeMethods.MONITORINFOF_PRIMARY) != 0
                    });
                }
            }

            return monitors;
        }

        /// <summary>
        /// Gets the DPI scale for a monitor, handling modern Windows multi-DPI scenarios.
        /// Returns scaling factors to convert from device pixels to WPF logical pixels.
        /// </summary>
        private static (double DpiScaleX, double DpiScaleY) GetScreenDpiScale(IntPtr hMonitor, NativeMethods.RECT monitorBounds)
        {
            double dpiScaleX = 1.0;
            double dpiScaleY = 1.0;

            try
            {
                if (NativeMethods.GetDpiForMonitor(hMonitor, NativeMethods.MDT_EFFECTIVE_DPI,
                    out uint dpiX, out uint dpiY) == 0)
                {
                    dpiScaleX = dpiX / 96.0;
                    dpiScaleY = dpiY / 96.0;
                }
            }
            catch
            {
                // Fall back to default DPI scaling if API call fails
            }

            return (dpiScaleX, dpiScaleY);
        }

        /// <summary>
        /// Represents information about a single monitor.
        /// </summary>
        private class MonitorInfo
        {
            public IntPtr Handle { get; set; }
            public Rect WorkingArea { get; set; }
            public bool IsPrimary { get; set; }
        }

        /// <summary>
        /// Native Windows API declarations for monitor enumeration and DPI detection.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            internal static extern bool EnumDisplayMonitors(
                IntPtr hdc,
                IntPtr lprcClip,
                MonitorEnumDelegate lpfnEnum,
                IntPtr dwData);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

            [DllImport("shcore.dll", SetLastError = true)]
            internal static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

            internal delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

            internal const uint MONITORINFOF_PRIMARY = 1;
            internal const int MDT_EFFECTIVE_DPI = 0;

            [StructLayout(LayoutKind.Sequential)]
            internal struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            internal struct MONITORINFOEX
            {
                public int cbSize;
                public RECT rcMonitor;
                public RECT rcWork;
                public uint dwFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string szDevice;
            }
        }
    }
}
