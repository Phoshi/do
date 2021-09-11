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
    public partial class CycleRangeAcquisition : UserControl
    {
        private readonly Duty.T _duty;
        private readonly Action _callback;
        private CycleRangeAcquisitionViewModel Context;
        public CycleRangeAcquisition(Duty.T duty, Task.T task, Action callback)
        {
            _duty = duty;
            _callback = callback;
            DataContext = Context = new();

            Context.Task = task;
            MapCycleTime(task);
            InitializeComponent();

            this.IsVisibleChanged += this.SetFocus();
        }

        private void MapCycleTime(Task.T task)
        {
            if (task.cycleRange.IsNever)
            {
                return;
            }

            if (task.cycleRange is CycleRange.T.After after)
            {
                Context.Start = CycleRange.CycleTime.stringify(after.Item);
            }

            if (task.cycleRange is CycleRange.T.Before before)
            {
                Context.End = CycleRange.CycleTime.stringify(before.Item);
            }

            if (task.cycleRange is CycleRange.T.Between between)
            {
                Context.Start = CycleRange.CycleTime.stringify(between.Item.Item1);
                Context.End = CycleRange.CycleTime.stringify(between.Item.Item2);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var from = this.Context.Start;
            var to = this.Context.End;

            var task = Task.setCycleRange(_range(), Context.Task);
            task = Task.setConfidence(Confidence.create("cycle", task.completed.Length), task);
            _duty.api.update(task, DateTime.Now);

            _callback();

            CycleRange.T _range()
            {
                if (!string.IsNullOrEmpty(Context.Start) && !string.IsNullOrEmpty(Context.End))
                {
                    return CycleRange.between(
                        CycleRange.CycleTime.parse(Context.Start),
                        CycleRange.CycleTime.parse(Context.End));
                }
                
                if (!string.IsNullOrEmpty(Context.End))
                {
                    return CycleRange.before(
                        CycleRange.CycleTime.parse(Context.End));
                }
                
                if (!string.IsNullOrEmpty(Context.Start))
                {
                    return CycleRange.after(
                        CycleRange.CycleTime.parse(Context.Start));
                }

                return CycleRange.never;
            }
        }

        private void OnLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Start.Focus();
            Start.SelectAll();
        }

        private void OnRight(object sender, ExecutedRoutedEventArgs e)
        {
            End.Focus();
            End.SelectAll();
        }

        private void OnActivate(object sender, ExecutedRoutedEventArgs e)
        {
            ButtonBase_OnClick(sender, e);
        }
    }
}