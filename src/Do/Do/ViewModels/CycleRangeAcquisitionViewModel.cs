using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do.Annotations;
using Tasks;

namespace Do.ViewModels
{
    public class CycleRangeAcquisitionViewModel : INotifyPropertyChanged
    {
        private Task.T _task;

        public Task.T Task
        {
            get => _task;
            set => PropertyChanged.ChangeAndNotify(ref _task, value, () => Task);
        }

        public string _start;

        public string Start
        {
            get => _start;
            set => PropertyChanged.ChangeAndNotify(ref _start, value, () => Start);
        }
        
        public string _end;

        public string End
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