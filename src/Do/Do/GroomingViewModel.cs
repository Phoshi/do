using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Do.Annotations;
using Duties;
using Tasks;

namespace Do
{
    public class GroomingViewModel : INotifyPropertyChanged
    {
        private IEnumerable<Task.T> _tasks;
        private Task.T _currentTask;
        private List<Task.T> _taskQueue;
        private Duty.T _duty;

        public IEnumerable<Task.T> Tasks
        {
            get => _tasks;
            set
            {
                PropertyChanged.ChangeAndNotify(ref _tasks, value, () => Tasks);
                OnPropertyChanged(nameof(TaskCount));
            }
        }
        
        public Duty.T Duty
        {
            get => _duty;
            set => PropertyChanged.ChangeAndNotify(ref _duty, value, () => Duty);
        }

        public List<Task.T> TaskQueue
        {
            get => _taskQueue;
            set
            {
                if (Equals(value, _taskQueue)) return;
                _taskQueue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TaskQueueCount));
            }
        }

        public int TaskCount => Tasks.Count();
        public int TaskQueueCount => TaskQueue.Count();

        public Task.T CurrentTask
        {
            get => _currentTask;
            set => PropertyChanged.ChangeAndNotify(ref _currentTask, value, () => CurrentTask);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}