using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Do.Commands;
using Do.Core;
using Do.Extensions;
using Duties;
using Tasks;

namespace Do.Pages
{
    public partial class TaskGrooming : UserControl
    {
        private readonly Action _callback;
        private GroomingViewModel Context { get; }
        public TaskGrooming(Duty.T duty, IEnumerable<Task.T> tasks, Action callback)
        {
            _callback = callback;
            DataContext = Context = new();
            
            Context.Duty = duty;
            
            var contextTasks = tasks.ToList();
            Context.Tasks = contextTasks;
            Context.TaskQueue = contextTasks.Skip(1).ToList();
            Context.CurrentTask = contextTasks.First();
            
            InitializeComponent();
        }

        private void AssignRequiredTasks()
        {
            var assignedTasks = Context.Duty.api.assignedTasks(Context.Tasks);
            foreach (var task in assignedTasks)
            {
                var t = Task.setConfidence(Confidence.full("assigned"), task);
                Context.Duty.api.update(t, DateTime.Now);
            }
        }

        private void NextTask()
        {
            if (Context.TaskQueue.Any())
            {
                var task = Context.TaskQueue.First();
                Context.CurrentTask = task;
                Context.TaskQueue = Context.TaskQueue.Skip(1).ToList();
            }
            else
            {
                AssignRequiredTasks();

                _callback();
            }
        }
        
        private void ScaleTaskAndContinue(double factor)
        {
            var task = Context.CurrentTask;
            var updatedTask = Task.setMomentum(Momentum.scale(factor, task.momentum), task);
            Context.Tasks = Context.Tasks.Update(updatedTask);

            Context.Duty.api.update(updatedTask, DateTime.Now);
            
            NextTask();
        }

        private void TaskGrooming_OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }

        private void OnLeftMajor(object sender, ExecutedRoutedEventArgs e)
        {
            ScaleTaskAndContinue(0.5);
        }

        private void OnLeftMinor(object sender, ExecutedRoutedEventArgs e)
        {
            ScaleTaskAndContinue(0.95);
        }

        private void OnRightMinor(object sender, ExecutedRoutedEventArgs e)
        {
            ScaleTaskAndContinue(1.0);
        }

        private void OnRightMajor(object sender, ExecutedRoutedEventArgs e)
        {
            ScaleTaskAndContinue(1.5);
        }
    }
}