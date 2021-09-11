using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Do.Extensions;
using Do.ViewModels;
using Duties;
using Tasks;

namespace Do.Pages
{
    public partial class RelevanceRangeAcquisition : UserControl
    {
        private readonly Duty.T _duty;
        private readonly Action _callback;
        private RelevanceRangeAcquisitionViewModel Context;
        public RelevanceRangeAcquisition(Duty.T duty, Task.T task, Action callback)
        {
            _duty = duty;
            _callback = callback;
            DataContext = Context = new();

            Context.Task = task;
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var from = this.Context.Start;
            var to = this.Context.End;

            var task = Task.setRelevanceRange(_range(), Context.Task);
            task = Task.setConfidence(Confidence.full("relevance"), task);
            _duty.api.update(task, DateTime.Now);

            _callback();

            RelevanceRange.T _range()
            {
                if (from.HasValue && to.HasValue)
                {
                    return RelevanceRange.between(from.Value, to.Value);
                }

                if (from.HasValue)
                {
                    return RelevanceRange.after(from.Value);
                }

                if (to.HasValue)
                {
                    return RelevanceRange.before(to.Value);
                }

                return RelevanceRange.always;
            }
        }

        private void OnLeft(object sender, ExecutedRoutedEventArgs e)
        {
            StartDatePicker.Focus();
        }

        private void OnRight(object sender, ExecutedRoutedEventArgs e)
        {
            EndDatePicker.Focus();
        }

        private void OnActivate(object sender, ExecutedRoutedEventArgs e)
        {
            ButtonBase_OnClick(sender, e);
        }

        private void RelevanceRangeAcquisition_OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }
    }
}