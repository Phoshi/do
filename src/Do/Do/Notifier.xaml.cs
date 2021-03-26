using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Do.Core;
using Duties;

namespace Do
{
    public partial class Notifier : Window
    {
        private readonly IEnumerable<Duty.T> _duties;
        private readonly Action _runner;
        private readonly DispatcherTimer _timer;

        public Notifier(IEnumerable<Duty.T> duties, Action runner)
        {
            _duties = duties;
            _runner = runner;
            _timer = new DispatcherTimer();
            InitializeComponent();
        }

        public void Notify()
        {
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += TimerOnTick;
            _timer.Start();
            
            CheckNotificationStatus();
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            CheckNotificationStatus();
        }

        private void CheckNotificationStatus()
        {
            var alerts = _duties.Select(d => d.api.alertState(DateTime.Now));

            if (alerts.Any(a => !a.IsNone))
            {
                var highestAlert = alerts.OrderByDescending(a => a.Tag).First();
                this.Button.Content = ButtonText(highestAlert);
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        public string ButtonText(Alert.T alert)
        {
            if (alert.IsInformationRequired) return "Info?";
            if (alert.IsReviewRequested) return "Review";
            if (alert.IsTaskAssigned) return "Assigned";
            if (alert.IsTaskRequired) return "Alert!";

            return "help how did i get here";
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _runner();
        }
        
        private void Notifier_OnLoaded(object sender, RoutedEventArgs e)
        {
            var maxHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var maxWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

            Top = maxHeight - Height;
            Left = maxWidth - Width;
        }
        
    }
}