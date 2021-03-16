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
    public partial class TaskGrooming : PageFunction<GroomingResponse>
    {
        private GroomingViewModel Context { get; }
        public TaskGrooming(Duty.T duty, IEnumerable<Task.T> tasks)
        {
            DataContext = Context = new();
            InitializeComponent();
            
            Context.Duty = duty;
            
            var contextTasks = tasks.ToList();
            Context.Tasks = contextTasks;
            Context.TaskQueue = contextTasks.Skip(1).ToList();
            Context.CurrentTask = contextTasks.First();

            TaskControl.Focus();

            UiCommands.ActionRaised += Action;
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
                var assignedTasks = Context.Duty.api.assignedTasks(Context.Tasks);
                foreach (var task in assignedTasks)
                {
                    var t = Task.setConfidence(Confidence.full("assigned"), task);
                    Context.Duty.api.update(t, DateTime.Now);
                    
                }
                this.OnReturn(new ReturnEventArgs<GroomingResponse>(new GroomingResponse()));
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

        private void Action(Command name)
        {
            switch (name)
            {
                case Command.LeftMajor:
                    ScaleTaskAndContinue(0.3);
                    return;
                case Command.LeftMinor:
                    ScaleTaskAndContinue(0.9);
                    return;
                case Command.RightMinor:
                    ScaleTaskAndContinue(1.1);
                    return;
                case Command.RightMajor:
                    ScaleTaskAndContinue(1.5);
                    return;
            }
        }

        private void TaskGrooming_OnUnloaded(object sender, RoutedEventArgs e)
        {
            UiCommands.ActionRaised -= Action;
        }
    }

    public class GroomingResponse
    {
    }
}