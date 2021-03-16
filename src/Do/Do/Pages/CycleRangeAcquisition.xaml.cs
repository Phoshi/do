using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Do.ViewModels;
using Duties;
using Tasks;

namespace Do.Pages
{
    public partial class CycleRangeAcquisition : PageFunction<CycleRangeAcquisitionReturn>
    {
        private readonly Duty.T _duty;
        private CycleRangeAcquisitionViewModel Context;
        public CycleRangeAcquisition(Duty.T duty, Task.T task)
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

            var task = Task.setCycleRange(_range(), Context.Task);
            task = Task.setConfidence(Confidence.full("cycle"), task);
            _duty.api.update(task, DateTime.Now);
            
            OnReturn(new ReturnEventArgs<CycleRangeAcquisitionReturn>());

            CycleRange.T _range()
            {
                if (!string.IsNullOrEmpty(Context.Start) && !string.IsNullOrEmpty(Context.End))
                {
                    return CycleRange.between(
                        CycleRange.CycleTime.parse(Context.Start),
                        CycleRange.CycleTime.parse(Context.End));
                }
                
                if (!string.IsNullOrEmpty(Context.End))
                {
                    return CycleRange.before(
                        CycleRange.CycleTime.parse(Context.End));
                }
                
                if (!string.IsNullOrEmpty(Context.Start))
                {
                    return CycleRange.after(
                        CycleRange.CycleTime.parse(Context.Start));
                }

                return CycleRange.never;
            }
        }

        private void CycleRangeAcquisition_OnLoaded(object sender, RoutedEventArgs e)
        {
            _after.Focus();
        }
    }

    public class CycleRangeAcquisitionReturn
    {
        
    }
}