using System.ComponentModel;
using System.Runtime.CompilerServices;
using Do.Annotations;

namespace Do.ViewModels
{
    public class NotifierViewModel : INotifyPropertyChanged
    {
        private string _buttonText;

        public string ButtonText
        {
            get => _buttonText;
            set => PropertyChanged.ChangeAndNotify(ref _buttonText, value, () => ButtonText);
        }

        public int _width = 20;

        public int Width
        {
            get => _width;
            set => PropertyChanged.ChangeAndNotify(ref _width, value, () => Width);
        }

        public string _primaryColour;

        public string PrimaryColour
        {
            get => _primaryColour;
            set => PropertyChanged.ChangeAndNotify(ref _primaryColour, value, () => PrimaryColour);
        }

        private bool _requestAttention;

        public bool RequestAttention
        {
            get => _requestAttention;
            set => PropertyChanged.ChangeAndNotify(ref _requestAttention, value, () => RequestAttention);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}