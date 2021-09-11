using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Do.Extensions;
using Do.ViewModels;
using Tasks;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;
using Task = System.Threading.Tasks.Task;

namespace Do.CaptureControls
{
    public partial class CaptureToScript : UserControl
    {
        private readonly string _scriptPath;
        private readonly Action _callback;
        private CaptureTaskViewModel Context;
        public CaptureToScript(string scriptPath, Action callback)
        {
            _scriptPath = scriptPath;
            _callback = callback;
            InitializeComponent();
            this.IsVisibleChanged += TaskName.SetFocus();
            this.DataContext = Context = new CaptureTaskViewModel();
        }

        private void Save()
        {
            var text = TaskName.Text;
            var thread = new Thread(() => Run(text));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _callback();
        }
        
        private string Run(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "PowerShell.exe";
                psi.Arguments =
                    @$"-file ""{_scriptPath}"" -text ""{command.Replace(@"""", @"\""").Replace("\r\n", "\n")}""";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;

                var process = new Process
                {
                    StartInfo = psi
                };

                process.Start();

                string err = string.Empty; //process.StandardError.ReadLine();

                if (!string.IsNullOrEmpty(err))
                {
                    return $"error: {err}";
                }

                var output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                return output;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
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