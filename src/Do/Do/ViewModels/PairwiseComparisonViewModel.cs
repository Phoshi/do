using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do.Annotations;
using Tasks;

namespace Do.ViewModels
{
    public class PairwiseComparisonViewModel : INotifyPropertyChanged
    {
        private IEnumerable<Task.T> _taskQueue;

        public IEnumerable<Task.T> TaskQueue
        {
            get => _taskQueue;
            set => PropertyChanged.ChangeAndNotify(ref _taskQueue, value, () => TaskQueue);
        }
        
        private Task.T _task;

        public Task.T Task
        {
            get => _task;
            set => PropertyChanged.ChangeAndNotify(ref _task, value, () => Task);
        }
        
        private Task.T _comparisonTask;

        public Task.T ComparisonTask
        {
            get => _comparisonTask;
            set => PropertyChanged.ChangeAndNotify(ref _comparisonTask, value, () => ComparisonTask);
        }
        
        private string _comparisonDescription;

        public string ComparisonDescription
        {
            get => _comparisonDescription;
            set => PropertyChanged.ChangeAndNotify(ref _comparisonDescription, value, () => ComparisonDescription);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}