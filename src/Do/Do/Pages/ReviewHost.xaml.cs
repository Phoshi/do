using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Do.Core;
using Duties;
using Tasks;

namespace Do.Pages
{
    public partial class ReviewHost : Page
    {
        private readonly Duty.T _duty;
        private readonly Review _hostingFor;

        private readonly List<(Func<bool>, Func<Page>)> _pages;

        public ReviewHost(Duty.T duty, Review hostingFor)
        {
            _pages = new List<(Func<bool>, Func<Page>)>
            {
                (HasAssignedTasks, AssignmentStep),
                (NeedCycleAcquisition, CycleStep),
                (NeedRelevanceAcquisition, RelevanceStep),
                (NeedPairwiseComparison, PairwiseComparisonStep),
                (() => true, GroomingStep)
            };
            
            _duty = duty;
            _hostingFor = hostingFor;
            InitializeComponent();

            Loaded += (_, _) => NextStep();
        }

        private void NextStep()
        {
            foreach (var (required, page) in _pages)
            {
                if (required())
                {
                    NavigationService.Navigate(page());
                    return;
                }
            }
        }
        
        private bool HasAssignedTasks()
            => AssignedTasks().Any();

        private bool NeedCycleAcquisition()
            => UnknownCycleTasks().Any();

        private Page CycleStep()
        {
            var step = new CycleRangeAcquisition(_duty, UnknownCycleTasks().First());
            step.Return += CycleReturn;
            return step;
        }

        private void CycleReturn(object sender, ReturnEventArgs<CycleRangeAcquisitionReturn> e)
        {
            NextStep();
        }
        
        private bool NeedRelevanceAcquisition()
            => UnknownRelevanceTasks().Any();

        private Page RelevanceStep()
        {
            var step = new RelevanceRangeAcquisition(_duty, UnknownRelevanceTasks().First());
            step.Return += RelevanceReturn;
            return step;
        }

        private void RelevanceReturn(object sender, ReturnEventArgs<RelevanceRangeAcquisitionReturn> e)
        {
            NextStep();
        }
        
        private Page AssignmentStep()
        {
            var step = new AssignedTasks(_duty, AssignedTasks());
            step.Return += AssignmentReturn;
            return step;
        }

        private void AssignmentReturn(object sender, ReturnEventArgs<AssignmentReturn> e)
        {
            _hostingFor.Close();
        }

        private bool NeedPairwiseComparison()
            => LowConfidenceTasks().Any();

        private Page PairwiseComparisonStep()
        {
            var step = new PairwiseComparison(_duty, LowConfidenceTasks());
            step.Return += PairwiseResponse;
            return step;
        }

        private void PairwiseResponse(object sender, ReturnEventArgs<PairwiseComparisonResponse> e)
        {
            NextStep();
        }

        private Page GroomingStep()
        {
            var review = _duty.api.review();
            var step = new TaskGrooming(_duty, review);
            step.Return += GroomingResponse;
            return step;
        }

        private void GroomingResponse(object sender, ReturnEventArgs<GroomingResponse> e)
        {
            NextStep();
        }

        private IEnumerable<Task.T> LowConfidenceTasks() =>
            _duty.api.lowConfidence(Confidence.create("importance", 1))
                .Concat(_duty.api.lowConfidence(Confidence.create("urgency", 1)));

        private IEnumerable<Task.T> AssignedTasks() =>
            _duty.api.highConfidence(Confidence.create("assigned", 0));

        private IEnumerable<Task.T> UnknownRelevanceTasks() =>
            _duty.api.lowConfidence(Confidence.create("relevance", 1));
        
        private IEnumerable<Task.T> UnknownCycleTasks() =>
            _duty.api.lowConfidence(Confidence.create("cycle", 1))
                .Where(t => t.completed.Length > 0);


        private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            OpenUrl(e.Parameter.ToString());
            e.Handled = true;
        }
        
        //https://stackoverflow.com/a/43232486
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void _host_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.WebRequest != null)
            {
                e.Cancel = true;
            }
        }
    }
}