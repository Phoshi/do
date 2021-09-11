using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Do.Core;
using Do.Extensions;
using Duties;
using MarkdownSource;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Optional.Collections;
using Tasks;
using Task = Tasks.Task;

namespace Do
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int SHOW_REVIEW_ID = 1;
        private const int SHOW_CAPTURE_ID = 2;
        private static Notifier _notifier;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleReview();
        }

        private static Review _review;
        public static void ToggleReview()
        {
            if (_review == null || !_review.IsLoaded)
            {
                Config.Active.ActiveDuties()
                    .SingleOrNone()
                    .MatchSome(duty =>
                    {
                        var review = new Review(duty);
                        review.Show();
                        review.Focus();
                        review.Activate();
                        review.GlobalActivate();

                        _review = review;
                    });
                return;
            }

            if (_review.IsVisible)
            {
                _review.Close();
            }
            else
            {
                _review.Show();
                _review.Focus();
                _review.GlobalActivate();
            }
        }
        
        private static Capture _capture;
        public static void ToggleCapture()
        {
            if (_capture == null || !_capture.IsLoaded)
            {
                var capture = new Capture(Config.Active.DefaultDuty());
                capture.Show();
                capture.Focus();
                capture.GlobalActivate();

                _capture = capture;
                return;
            }

            if (_capture.IsVisible)
            {
                _capture.Close();
            }
            else
            {
                _capture.Show();
                _capture.Focus();
                _capture.Activate();
                _capture.GlobalActivate();
            }
        }

        public static void BumpNotifier()
        {
            _notifier.CheckNotificationStatus();
        }

        // DLL libraries used to manage hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312)
            {
                if (wParam.ToInt32() == SHOW_REVIEW_ID)
                    ToggleReview();
                if (wParam.ToInt32() == SHOW_CAPTURE_ID)
                {
                    if (_review?.IsVisible ?? false)
                    {
                        ToggleReview();
                    }
                    else
                    {
                        ToggleCapture();
                    }
                }
            }

            return IntPtr.Zero;
        }

        [Flags]
        public enum Modifiers
        {
            NoMod = 0x0000,
            Alt = 0x0001,
            Ctrl = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var source = PresentationSource.FromVisual(this as Visual) as HwndSource;
            if (source == null)
                throw new Exception("Could not create hWnd source from window.");
            source.AddHook(WndProc);

            RegisterHotKey(new WindowInteropHelper(this).Handle, SHOW_CAPTURE_ID, (int) (Modifiers.Ctrl | Modifiers.Alt),
                0x44 /*D*/);
            
            RegisterHotKey(new WindowInteropHelper(this).Handle, SHOW_CAPTURE_ID, (int) (Modifiers.NoMod),
                0x81 /*F18*/);

            _notifier = new Notifier(Config.Active.ActiveDuties(), ToggleReview);
            _notifier.Notify();

            Visibility = Visibility.Hidden;
        }
    }
}