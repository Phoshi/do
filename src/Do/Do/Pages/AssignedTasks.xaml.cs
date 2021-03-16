using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Do.ViewModels;
using Duties;
using Tasks;

namespace Do
{
    public partial class AssignedTasks : PageFunction<AssignmentReturn>
    {
        private AssignedTasksViewModel Context { get; }
        public AssignedTasks(Duty.T duty, IEnumerable<Task.T> tasks)
        {
            DataContext = Context = new();
            Context.Tasks = tasks;
            Context.CurrentTask = tasks.First();
            Context.Duty = duty;
            
            InitializeComponent();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var now = DateTime.Now;
            var task = e.Parameter as Task.T;
            Context.Duty.api.complete(task, now);
            task = Task.setConfidence(Confidence.zero("assigned"), task);
            Context.Duty.api.update(task, DateTime.Now);
            
            Context.Tasks = Context.Tasks.Where(t => t.filepath != task.filepath);
            
            if (!Context.Tasks.Any())
                OnReturn(new ReturnEventArgs<AssignmentReturn>());
        }
    }

    public class AssignmentReturn
    {
    }
}