using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Do.Core;
using Duties;
using Humanizer;
using Tasks;

namespace Do.Controls
{
    public partial class TaskControl : UserControl
    {
        public TaskControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
        
        public static readonly DependencyProperty TaskProperty =
            DependencyProperty.Register(
                "Task",
                typeof(Task.T),
                typeof(TaskControl),
                new PropertyMetadata(OnChange));
        
        public static readonly DependencyProperty DutyProperty =
            DependencyProperty.Register(
                "Duty",
                typeof(Duty.T),
                typeof(TaskControl),
                new PropertyMetadata(OnChange));

        private static void OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskControl c)
                c.OnTaskUpdate();
        }

        private void OnTaskUpdate()
        {
            if (Duty != null)
            {
                var weight = Duty.api.weight(Task, DateTime.Now);
                Weight.Text = Tasks.Weight.stringify(weight.Item1, weight.Item2);
            }

            if (Task.completed.Any())
            {
                var lastCompletion = Task.completed.OrderBy(d => d).Last();
                Completions.Text = $"Last completed {lastCompletion.Humanize()}{(Task.completed.Length > 1 ? $" (+{(Task.completed.Length - 1).ToWords()} more)" : string.Empty)}";
                Due.Text = Stringify(Task.cycleRange, lastCompletion);
            }
            else
            {
                Completions.Text = "Incomplete";
                if (Task.relevanceRange.IsBefore || Task.relevanceRange.IsBetween)
                {
                    var deadline =
                        Task.relevanceRange is RelevanceRange.T.Before b
                            ? b.Item
                            : Task.relevanceRange is RelevanceRange.T.Between be
                                ? be.Item.Item2
                                : DateTime.Now;

                    if (deadline > DateTime.Now)
                    {
                        Due.Text = $"Due {deadline.Humanize()} ({deadline:yyyy-MM-dd})";
                    }
                    else
                    {
                        Due.Text = $"Due now";
                    }
                    
                }
                else
                {
                    Due.Text = "";
                }
            }
        }

        private string Stringify(CycleRange.T cycleRange, DateTime lastCompletion)
        {
            if (cycleRange is CycleRange.T.Before b)
            {
                return $"Due {CycleRange.CycleTime.toDateTime(b.Item, lastCompletion).Humanize()}";
            }

            if (cycleRange is CycleRange.T.Between bet)
            {
                return $"Due {CycleRange.CycleTime.toDateTime(bet.Item.Item2, lastCompletion).Humanize()}";
            }
            
            return String.Empty;
        }

        public Task.T Task
        {
            get => (Task.T)GetValue(TaskProperty);
            set => SetValue(TaskProperty, value);
        }
        
        public Duty.T Duty
        {
            get => (Duty.T)GetValue(DutyProperty);
            set => SetValue(DutyProperty, value);
        }
    }
}