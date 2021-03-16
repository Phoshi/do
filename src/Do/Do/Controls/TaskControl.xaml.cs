using System.Windows;
using System.Windows.Controls;
using Do.Core;
using Tasks;

namespace Do.Controls
{
    public partial class TaskControl : UserControl
    {
        public TaskControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
        
        public static readonly DependencyProperty TaskProperty =
            DependencyProperty.Register(
                "Task",
                typeof(Task.T),
                typeof(TaskControl),
                new PropertyMetadata(OnChange));

        private static void OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskControl c)
                c.OnTaskUpdate();
        }

        private void OnTaskUpdate()
        {
            var weight = ReviewSelection.taskWeight(Task);
            Weight.Text = weight.ToString();
        }

        public Task.T Task
        {
            get => (Task.T)GetValue(TaskProperty);
            set => SetValue(TaskProperty, value);
        }
    }
}