using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Markdig;
using Markdig.Wpf;

namespace Do.Controls
{
    public partial class MarkdownView : UserControl
    {
        public MarkdownView()
        {
            InitializeComponent();
        }
        
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MarkdownView),
                new FrameworkPropertyMetadata(MarkdownChanged));
        
        public string Markdown
        {
            get { return (string) GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        private static void MarkdownChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (MarkdownView) sender;
            control.RefreshDocument();
        }

        private static readonly MarkdownPipeline DefaultPipeline =
            new MarkdownPipelineBuilder().UseSupportedExtensions().Build();

        private void RefreshDocument()
        {
            var doc = Markdown != null ? Markdig.Wpf.Markdown.ToFlowDocument(Markdown, DefaultPipeline) : null;

            Viewer.Document = doc;
        }
    }
}