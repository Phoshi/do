using System;
using System.Windows;
using System.Windows.Input;
using Do.CaptureControls;
using Duties;
using MarkdownSource;

namespace Do
{
    public partial class Capture : Window
    {
        private readonly Duty.T _duty;
        private bool _isDispatch = true;
        public Capture(Duty.T duty)
        {
            _duty = duty;
            InitializeComponent();
            Content.Content = new CaptureDispatch(duty);
        }

        private void Capture_OnLoaded(object sender, RoutedEventArgs e)
        {
            var maxHeight = System.Windows.SystemParameters.WorkArea.Bottom;
            var maxWidth = System.Windows.SystemParameters.WorkArea.Right;

            Top = maxHeight - Height;
            Left = maxWidth - Width;
        }

        private void Run(CaptureMeta capture)
        {
            if (capture.captureTask)
            {
                var task = new CaptureTask(_duty, Close);
                Content.Content = task;
                task.TaskName.Focus();
            } 
            else if (capture.beginReview)
            {
                MainWindow.ToggleReview();
                MainWindow.ToggleCapture();
            } 
            else if (!string.IsNullOrEmpty(capture.scriptPath))
            {
                var task = new CaptureToScript(capture.scriptPath, Close);
                Content.Content = task;
                task.TaskName.Focus();
            }
        }

        private void Capture_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isDispatch)
                return;
            _isDispatch = false;

            foreach (var capture in _duty.dutyMeta.capture)
            {
                var key = Enum.Parse<Key>(capture.key);
                if (e.Key == key)
                {
                    Run(capture);
                }
            }
        }
    }
}