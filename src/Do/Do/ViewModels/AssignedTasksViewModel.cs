using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do.Annotations;
using Duties;
using Optional.Collections;
using Optional.Unsafe;
using Tasks;

namespace Do.ViewModels
{
    public class AssignedTasksViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Task.T CurrentTask
        {
            get => _tasks.FirstOrNone(t => t.Current).Else(_tasks.FirstOrNone()).ValueOrFailure().Task;
        }

        private IEnumerable<AssignedTask> _tasks;
        public IEnumerable<AssignedTask> Tasks
        {
            get => _tasks;
            set => PropertyChanged.ChangeAndNotify(ref _tasks, value, () => Tasks);
        }

        private Duty.T _duty;
        public Duty.T Duty
        {
            get => _duty;
            set => PropertyChanged.ChangeAndNotify(ref _duty, value, () => Duty);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public record AssignedTask(Task.T Task, bool Current);
}