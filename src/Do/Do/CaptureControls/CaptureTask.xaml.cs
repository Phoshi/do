using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Do.Extensions;
using Do.ViewModels;
using Duties;
using Tasks;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

namespace Do.CaptureControls
{
    public partial class CaptureTask : UserControl
    {
        private readonly Duty.T _duty;
        private readonly Action _callback;
        private CaptureTaskViewModel Context;
        public CaptureTask(Duty.T duty, Action callback)
        {
            _duty = duty;
            _callback = callback;
            InitializeComponent();
            this.IsVisibleChanged += TaskName.SetFocus();
            this.DataContext = Context = new CaptureTaskViewModel();
        }

        private void Save()
        {
            var task = Task.parse(DateTime.Now, TaskName.Text);
            _duty.api.update(task, DateTime.Now);
            _callback();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void TaskName_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Save();
            }
        }
    }
}