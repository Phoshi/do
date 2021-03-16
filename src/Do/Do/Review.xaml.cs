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

            _main.Navigate(new ReviewHost(duty, this));
        }

        private void Raise(Command name)
        {
            UiCommands.Raise(name);
        }

        private void OnLeftMajor(object sender, ExecutedRoutedEventArgs e) => Raise(Command.LeftMajor);

        private void OnLeftMinor(object sender, ExecutedRoutedEventArgs e) => Raise(Command.LeftMinor);

        private void OnRightMinor(object sender, ExecutedRoutedEventArgs e) => Raise(Command.RightMinor);

        private void OnRightMajor(object sender, ExecutedRoutedEventArgs e) => Raise(Command.RightMajor);

        private void Review_OnLoaded(object sender, RoutedEventArgs e)
        {
            var maxHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var maxWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

            Top = maxHeight - Height;
            Left = maxWidth - Width;
        }
    }
    
}