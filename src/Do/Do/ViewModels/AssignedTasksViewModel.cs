using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do.Annotations;
using Duties;
using Tasks;

namespace Do.ViewModels
{
    public class AssignedTasksViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Task.T _task;
        public Task.T CurrentTask
        {
            get => _task;
            set => PropertyChanged.ChangeAndNotify(ref _task, value, () => CurrentTask);
        }

        private IEnumerable<Task.T> _tasks;
        public IEnumerable<Task.T> Tasks
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
}