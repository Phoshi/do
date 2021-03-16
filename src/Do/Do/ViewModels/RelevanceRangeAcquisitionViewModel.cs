using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do.Annotations;
using Tasks;

namespace Do.ViewModels
{
    public class RelevanceRangeAcquisitionViewModel : INotifyPropertyChanged
    {
        private Task.T _task;

        public Task.T Task
        {
            get => _task;
            set => PropertyChanged.ChangeAndNotify(ref _task, value, () => Task);
        }

        public DateTime? _start;

        public DateTime? Start
        {
            get => _start;
            set => PropertyChanged.ChangeAndNotify(ref _start, value, () => Start);
        }
        
        public DateTime? _end;

        public DateTime? End
        {
            get => _end;
            set => PropertyChanged.ChangeAndNotify(ref _end, value, () => End);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}