﻿using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Input;
using System.Windows.Threading;
using ColorPicker.Helpers;
using ColorPicker.Settings;
using static ColorPicker.Win32Apis;

namespace ColorPicker.Mouse
{
    [Export(typeof(IMouseInfoProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MouseInfoProvider : IMouseInfoProvider
    {
        private const int MousePullInfoIntervalInMs = 10;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly MouseHook _mouseHook;
        private readonly IUserSettings _userSettings;
        private System.Windows.Point _previousMousePosition = new System.Windows.Point(-1, 1);
        private Color _previousColor = Color.Transparent;

        [ImportingConstructor]
        public MouseInfoProvider(AppStateHandler appStateMonitor, IUserSettings userSettings)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(MousePullInfoIntervalInMs);
            _timer.Tick += Timer_Tick;

            appStateMonitor.AppShown += AppStateMonitor_AppShown;
            appStateMonitor.AppClosed += AppStateMonitor_AppClosed;
            appStateMonitor.AppHidden += AppStateMonitor_AppClosed;
            _mouseHook = new MouseHook();
            _userSettings = userSettings;
        }

        public event EventHandler<Color> MouseColorChanged;

        public event EventHandler<System.Windows.Point> MousePositionChanged;

        public event EventHandler<Tuple<System.Windows.Point, bool>> OnMouseWheel;

        public event MouseUpEventHandler OnMouseDown;

        public System.Windows.Point CurrentPosition
        {
            get
            {
                return _previousMousePosition;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateMouseInfo();
        }

        private void UpdateMouseInfo()
        {
            var mousePosition = GetCursorPosition();
            if (_previousMousePosition != mousePosition)
            {
                _previousMousePosition = mousePosition;
                MousePositionChanged?.Invoke(this, mousePosition);
            }

            var color = GetPixelColor(mousePosition);
            if (_previousColor != color)
            {
                _previousColor = color;
                MouseColorChanged?.Invoke(this, color);
            }
        }

        private static Color GetPixelColor(System.Windows.Point mousePosition)
        {
            var rect = new Rectangle((int)mousePosition.X, (int)mousePosition.Y, 1, 1);
            var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            return bmp.GetPixel(0, 0);
        }

        private static System.Windows.Point GetCursorPosition()
        {
            GetCursorPos(out PointInter lpPoint);
            return (System.Windows.Point)lpPoint;
        }

        private void AppStateMonitor_AppClosed(object sender, EventArgs e)
        {
            _timer.Stop();
            _previousMousePosition = new System.Windows.Point(-1, 1);
            _mouseHook.OnMouseDown -= MouseHook_OnMouseDown;
            _mouseHook.OnMouseWheel -= MouseHook_OnMouseWheel;

            if (_userSettings.ChangeCursor.Value)
            {
                CursorManager.RestoreOriginalCursors();
            }
        }

        private void AppStateMonitor_AppShown(object sender, EventArgs e)
        {
            UpdateMouseInfo();
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }

            _mouseHook.OnMouseDown += MouseHook_OnMouseDown;
            _mouseHook.OnMouseWheel += MouseHook_OnMouseWheel;

            if (_userSettings.ChangeCursor.Value)
            {
                CursorManager.SetColorPickerCursor();
            }
        }

        private void MouseHook_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0)
            {
                return;
            }

            var zoomIn = e.Delta > 0;
            OnMouseWheel?.Invoke(this, new Tuple<System.Windows.Point, bool>(_previousMousePosition, zoomIn));
        }

        private void MouseHook_OnMouseDown(object sender, Point p)
        {
            OnMouseDown?.Invoke(this, p);
        }
    }
}
