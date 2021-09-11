using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Do.Core;
using Do.ViewModels;
using Duties;

namespace Do
{
    public partial class Notifier : Window
    {
        private readonly IEnumerable<Duty.T> _duties;
        private readonly Action _runner;
        private readonly DispatcherTimer _timer;

        private NotifierViewModel Context;

        public Notifier(IEnumerable<Duty.T> duties, Action runner)
        {
            _duties = duties;
            _runner = runner;
            DataContext = Context = new();
            _timer = new DispatcherTimer();
            InitializeComponent();
        }

        public void Notify()
        {
            _timer.Interval = TimeSpan.FromSeconds(30);
            _timer.Tick += TimerOnTick;
            _timer.Start();
            
            CheckNotificationStatus();
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            CheckNotificationStatus();
        }

        public void CheckNotificationStatus()
        {
            var alerts = _duties.Select(d => d.api.alertState(DateTime.Now)).ToList();

            if (alerts.Any(a => !a.IsNone))
            {
                var highestAlert = alerts.OrderByDescending(a => a.Tag).First();
                this.Context.ButtonText = ButtonText(highestAlert);
                this.Context.PrimaryColour = ButtonColour(highestAlert);
                this.Context.RequestAttention = highestAlert.IsTaskRequired;
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        private string ButtonColour(Alert.T alert)
        {
            if (alert.IsInformationRequired) return "#A3BE8C";
            if (alert.IsReviewRequested) return "#B48EAD";
            if (alert.IsTaskAssigned) return "#D08770";
            if (alert.IsTaskRequired) return "#BF616A";

            return "#000000";
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
            var workArea = SystemParameters.WorkArea;

            Height = workArea.Height + 1;
            Top = workArea.Top;
            Left = workArea.Right - Width + 1;
        }
    }
}