using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xaml;
using Duties;
using XamlReader = System.Windows.Markup.XamlReader;

namespace Do.CaptureControls
{
    public partial class CaptureDispatch : UserControl
    {
        private const string Template = @"<Paragraph FontSize=""24"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">{0}</Paragraph>";
        private readonly Duty.T _duty;

        public CaptureDispatch(Duty.T duty)
        {
            _duty = duty;
            InitializeComponent();
            this.Host.Document = Doc();
        }

        private FlowDocument Doc()
        {
            var document = new FlowDocument();

            foreach (var capture in _duty.dutyMeta.capture)
            {
                document.Blocks.Add((Block)XamlReader.Parse(string.Format(Template, capture.description)));
            }
            return document;
        }
    }
}