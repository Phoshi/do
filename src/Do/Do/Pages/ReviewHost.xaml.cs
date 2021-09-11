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
    public partial class ReviewHost : UserControl
    {
        private readonly Duty.T _duty;
        private readonly Review _hostingFor;

        private readonly List<(Func<bool>, Func<UserControl>)> _pages;

        public ReviewHost(Duty.T duty, Review hostingFor)
        {
            _pages = new List<(Func<bool>, Func<UserControl>)>
            {
                (ShouldAssignTasks, AssignTasks),
                (NeedCycleAcquisition, CycleStep),
                (NeedRelevanceAcquisition, RelevanceStep),
                (NeedPairwiseComparison, PairwiseComparisonStep),
                (CanGroom, GroomingStep),
                (HasAssignedTasks, AssignmentStep),
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
                    var pageInstance = page();
                    if (pageInstance == null)
                    {
                        NextStep();
                        return;
                    }
                    Content.Content = pageInstance;
                    
                    return;
                }
            }
            
            _hostingFor.Close();
        }

        private bool ShouldAssignTasks()
        {
            return _duty.api.review().Any(t =>
                _duty.api.weight(t, DateTime.Now).Item1.IsMaximum &&
                LowConfidenceSelection.lowerConfidenceThan(Confidence.full("assigned"), t));
        }
        
        private bool HasAssignedTasks()
            => AssignedTasks().Any();

        private bool NeedCycleAcquisition()
            => _cycled < 5 && UnknownCycleTasks().Any();

        private int _cycled = 0;
        private UserControl CycleStep()
        {
            _cycled++;
            var step = new CycleRangeAcquisition(_duty, UnknownCycleTasks().First(), NextStep);
            return step;
        }
        
        private bool NeedRelevanceAcquisition()
            => UnknownRelevanceTasks().Any();

        private UserControl AssignTasks()
        {
            var assignedTasks = _duty.api.assignedTasks(_duty.tasks);
            foreach (var task in assignedTasks)
            {
                if (_duty.api.weight(task, DateTime.Now).Item1.IsMaximum)
                {
                    var t = Task.setConfidence(Confidence.full("assigned"), task);
                    _duty.api.update(t, DateTime.Now);
                }
            }
            return null;
        }

        private UserControl RelevanceStep()
        {
            var step = new RelevanceRangeAcquisition(_duty, UnknownRelevanceTasks().First(), NextStep);
            return step;
        }

        private UserControl AssignmentStep()
        {
            var step = new AssignedTasks(_duty, AssignedTasks(), _hostingFor.Close);
            return step;
        }

        private bool NeedPairwiseComparison()
            => LowConfidenceTasks().Any();

        private UserControl PairwiseComparisonStep()
        {
            var step = new PairwiseComparison(_duty, LowConfidenceTasks(), NextStep);
            return step;
        }

        private bool CanGroom()
            => _duty.api.review().Any(t => !_duty.api.weight(t, DateTime.Now).Item1.IsMaximum);
        private UserControl GroomingStep()
        {
            var review = _duty.api.review();
            var step = new TaskGrooming(_duty, review, NextStep);
            return step;
        }

        private IEnumerable<Task.T> LowConfidenceTasks() =>
            _duty.api.lowConfidence(Confidence.create("importance", 1))
                .Concat(_duty.api.lowConfidence(Confidence.create("urgency", 1)))
                .Distinct();

        private IEnumerable<Task.T> AssignedTasks() =>
            _duty.api.highConfidence(Confidence.create("assigned", 0))
                .OrderByDescending(t => _duty.api.weight(t, DateTime.Now));

        private IEnumerable<Task.T> UnknownRelevanceTasks() =>
            _duty.api.lowConfidence(Confidence.create("relevance", 1));
        
        private IEnumerable<Task.T> UnknownCycleTasks() =>
            _duty.api.lowConfidence(Confidence.create("cycle", Double.PositiveInfinity))
                .Where(t => t.completed.Length > Math.Pow(Confidence.confidenceLevel("cycle", t.confidence), 2));


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
    }
}