using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Do.Commands;
using Do.Core;
using Do.Extensions;
using Do.Pages;
using Duties;
using Tasks;

namespace Do
{
    public partial class Review : Window
    {
        public Review(Duty.T duty)
        {
            InitializeComponent();
            DataContext = this;

            var page = new ReviewHost(duty, this);
            _main.Content = page;
        }

        private void Review_OnLoaded(object sender, RoutedEventArgs e)
        {
            var maxHeight = System.Windows.SystemParameters.WorkArea.Bottom;
            var maxWidth = System.Windows.SystemParameters.WorkArea.Right;

            Top = maxHeight - Height + 1;
            Left = maxWidth - Width + 1;
        }

        private void Review_OnClosed(object? sender, EventArgs e)
        {
            MainWindow.BumpNotifier();
        }
    }
    
}