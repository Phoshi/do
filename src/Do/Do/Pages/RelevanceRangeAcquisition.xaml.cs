using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Do.ViewModels;
using Duties;
using Tasks;

namespace Do.Pages
{
    public partial class RelevanceRangeAcquisition : PageFunction<RelevanceRangeAcquisitionReturn>
    {
        private readonly Duty.T _duty;
        private RelevanceRangeAcquisitionViewModel Context;
        public RelevanceRangeAcquisition(Duty.T duty, Task.T task)
        {
            _duty = duty;
            DataContext = Context = new();

            Context.Task = task;
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var from = this.Context.Start;
            var to = this.Context.End;

            var task = Task.setRelevanceRange(_range(), Context.Task);
            task = Task.setConfidence(Confidence.full("relevance"), task);
            _duty.api.update(task, DateTime.Now);
            
            OnReturn(new ReturnEventArgs<RelevanceRangeAcquisitionReturn>());

            RelevanceRange.T _range()
            {
                if (from.HasValue && to.HasValue)
                {
                    return RelevanceRange.between(from.Value, to.Value);
                }

                if (from.HasValue)
                {
                    return RelevanceRange.after(from.Value);
                }

                if (to.HasValue)
                {
                    return RelevanceRange.before(to.Value);
                }

                return RelevanceRange.always;
            }
        }
    }

    public class RelevanceRangeAcquisitionReturn
    {
        
    }
}