using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace Do.Extensions
{
    public static class WindowFocusExtension
    {
        #region Constants

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        #endregion

        public static void GlobalActivate(this Window w) {
            var interopHelper = new WindowInteropHelper(w);
            var _handle = interopHelper.Handle;
            if (_handle == GetForegroundWindow()) {
                return;
            }

            attemptSetForeground(_handle, GetForegroundWindow());
            bool meAttachedToFore, foreAttachedToTarget;

            IntPtr foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            IntPtr thisThread = (IntPtr) Thread.CurrentThread.ManagedThreadId;
            IntPtr targetThread = GetWindowThreadProcessId(_handle, IntPtr.Zero);

            meAttachedToFore = AttachThreadInput(thisThread, foregroundThread, true);
            foreAttachedToTarget = AttachThreadInput(foregroundThread, targetThread, true);
            IntPtr foreground = GetForegroundWindow();
            BringWindowToTop(_handle);
            for (int i = 0; i < 5; i++) {
                attemptSetForeground(_handle, foreground);
                if (GetForegroundWindow() == _handle) {
                    break;
                }
            }

            //SetForegroundWindow(_handle);
            AttachThreadInput(foregroundThread, thisThread, false);
            AttachThreadInput(foregroundThread, targetThread, false);

            if (GetForegroundWindow() != _handle) {
                // Code by Daniel P. Stasinski
                // Converted to C# by Kevin Gale
                IntPtr Timeout = IntPtr.Zero;
                SystemParametersInfo(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, Timeout, 0);
                SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, IntPtr.Zero, 0x1);
                BringWindowToTop(_handle); // IE 5.5 related hack
                SetForegroundWindow(_handle);
                SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, Timeout, 0x1);
            }

            if (meAttachedToFore) {
                AttachThreadInput(thisThread, foregroundThread, false);
            }
            if (foreAttachedToTarget) {
                AttachThreadInput(foregroundThread, targetThread, false);
            }
        }

        private static void attemptSetForeground(IntPtr target, IntPtr foreground) {
            SetForegroundWindow(target);
            Thread.Sleep(10);
            IntPtr newFore = GetForegroundWindow();
            if (newFore == target) {
                return;
            }
            if (newFore != foreground && target == GetWindow(newFore, 4)) {
                //4 is GW_OWNER - the window parent
                return;
            }
            return;
        }

        #region Imports

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);
        
        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
        
        public const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        public const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        
        [DllImport("user32.dll")]
        public static extern
        bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr handle, uint command);

        #endregion
    }
}