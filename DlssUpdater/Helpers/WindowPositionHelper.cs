﻿using System;
using System.Windows;
using System.Windows.Interop;

namespace DlssUpdater.Helpers;

public static class WindowPositionHelper
{
    public static void CenterWindowToParent(Window childWindow, Window parentWindow)
    {
        // Ensure the child window is fully rendered before positioning
        childWindow.SizeChanged += (sender, args) =>
        {
            // Get the DPI scaling factors for both the parent and the child window
            var parentDpiScale = GetDpiScale(parentWindow);
            var childDpiScale = GetDpiScale(childWindow);

            // Check if the parent window is maximized
            bool isParentMaximized = parentWindow.WindowState == WindowState.Maximized;

            // Get the parent window's bounds
            Rect parentBounds;
                // Get the parent window's position and size in screen coordinates
                var parentPosition = parentWindow.PointToScreen(new Point(0, 0));
                double parentWidthInPx = parentWindow.ActualWidth * parentDpiScale.X;
                double parentHeightInPx = parentWindow.ActualHeight * parentDpiScale.Y;
                parentBounds = new Rect(parentPosition.X, parentPosition.Y, parentWidthInPx, parentHeightInPx);

            // Get the child window's actual size in physical pixels
            double childWidthInPx = childWindow.ActualWidth * childDpiScale.X;
            double childHeightInPx = childWindow.ActualHeight * childDpiScale.Y;

            // Ensure the child window size is available
            if (childWidthInPx == 0 || childHeightInPx == 0)
            {
                return; // Wait until the size is properly calculated
            }

            // Calculate the centered position relative to the parent
            double childLeft = parentBounds.X + (parentBounds.Width - childWidthInPx) / 2;
            double childTop = parentBounds.Y + (parentBounds.Height - childHeightInPx) / 2;

            // Convert the position back to logical units for the child window
            childWindow.Left = childLeft / childDpiScale.X;
            childWindow.Top = childTop / childDpiScale.Y;
        };
    }

    private static Point GetDpiScale(Window window)
    {
        // Get the window's presentation source
        var source = PresentationSource.FromVisual(window);
        if (source?.CompositionTarget != null)
        {
            // Return the DPI scale factor (logical units to physical pixels)
            return new Point(source.CompositionTarget.TransformToDevice.M11,
                             source.CompositionTarget.TransformToDevice.M22);
        }
        return new Point(1.0, 1.0); // Default scale if source is not available
    }

    // P/Invoke declarations to work with monitor information
    [System.Runtime.InteropServices.DllImport("User32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions dwFlags);

    [System.Runtime.InteropServices.DllImport("User32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    public class MONITORINFO
    {
        public int cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
    }

    private enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }
}
