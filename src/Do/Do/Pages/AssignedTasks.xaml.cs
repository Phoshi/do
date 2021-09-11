using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Do.Extensions;
using Do.ViewModels;
using Duties;
using Tasks;

namespace Do
{
    public partial class AssignedTasks : UserControl
    {
        private readonly Action _callback;
        private AssignedTasksViewModel Context { get; }
        public AssignedTasks(Duty.T duty, IEnumerable<Task.T> tasks, Action callback)
        {
            _callback = callback;
            DataContext = Context = new();
            Context.Tasks = tasks.Select((t, i) => new AssignedTask(t, i == 0));
            Context.Duty = duty;
            
            InitializeComponent();
            IsVisibleChanged += this.SetFocus();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var now = DateTime.Now;
            var task = e.Parameter as AssignedTask;
            CompleteTask(task.Task, now);
        }

        private void ActivateTask(Task.T task)
        {
            Context.Tasks = Context.Tasks.Select(t => new AssignedTask(t.Task, t.Task.Equals(task)));
        }

        private void CompleteTask(Task.T task, DateTime now)
        {
            task = Task.setConfidence(Confidence.zero("assigned"), task);
            Context.Duty.api.complete(task, now);

            Context.Tasks = Context.Tasks.Where(t => t.Task.filepath != task.filepath).ToList();
            if (Context.Tasks.Any())
            {
                ActivateTask(Context.Tasks.First().Task);
            }

            if (!Context.Tasks.Any())
                _callback();
        }

        private void AssignedTasks_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return){
                CompleteTask(Context.CurrentTask, DateTime.Now);
                e.Handled = true;
            }
        }
    }
}