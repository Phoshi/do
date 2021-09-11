using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do.Annotations;

namespace Do.ViewModels
{
    public class CaptureTaskViewModel : INotifyPropertyChanged
    {
        private string _taskDef;

        public string TaskDefinition
        {
            get => _taskDef;
            set => PropertyChanged.ChangeAndNotify(ref _taskDef, value, () => TaskDefinition);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}