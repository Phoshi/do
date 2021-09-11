using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Do.Commands;
using Do.ViewModels;
using Duties;
using Microsoft.FSharp.Collections;
using Optional;
using Optional.Unsafe;
using Tasks;

namespace Do.Pages
{
    public partial class PairwiseComparison : UserControl
    {
        private readonly List<string> _measures = new List<string>
        {
            "importance",
            "urgency"
        };
        
        private readonly Duty.T _duty;
        private readonly Action _callback;

        private PairwiseComparisonViewModel Context;

        public PairwiseComparison(Duty.T duty, IEnumerable<Task.T> lowConfidenceTasks, Action callback)
        {
            _duty = duty;
            _callback = callback;
            DataContext = Context = new();
            InitializeComponent();

            Context.TaskQueue = lowConfidenceTasks;
            Context.Task = PopTask();
        }

        private Task.T PopTask()
        {
            var task = Context.TaskQueue.First();
            Context.TaskQueue = Context.TaskQueue.Skip(1);
            
            return task;
        }

        private void PersistSelection()
        {
            var task = Context.Task;
            if (ComparisonType == "urgency")
            {
                task = Task.setUrgency(Urgency.create(ComparisonResult()), task);
            } 
            else if (ComparisonType == "importance")
            {
                task = Task.setImportance(Importance.create(ComparisonResult()), task);
            }

            task = Task.setConfidence(Confidence.full(ComparisonType), task);

            Context.Task = task;
            _duty.api.update(task, DateTime.Now);
        }

        private void SetNextTask()
        {
            var lowConfidenceMeasures =
                _measures
                    .Where(_activeTaskIsLowConfidence)
                    .ToList();

            if (lowConfidenceMeasures.Any())
            {
                ComparisonType = lowConfidenceMeasures.First();
                ComparisonLower = null;
                ComparisonUpper = null;
                Context.ComparisonDescription = $"Which task has greater {ComparisonType}?";
                SetNextComparison();
            }
            else if (Context.TaskQueue.Any())
            {
                Context.Task = PopTask();
                SetNextTask();
            }
            else
            {
                _callback();
            }


            bool _activeTaskIsLowConfidence(string measure)
            {
                var confidences = SeqModule.OfList(Context.Task.confidence);
                return Confidence.confidenceLevel(measure, confidences) == 0;
            }

        }

        private double ComparisonResult()
        {
            if (ComparisonLower.HasValue && ComparisonUpper.HasValue)
            {
                return (ComparisonLower.Value + ComparisonUpper.Value) / 2;
            }

            if (ComparisonLower.HasValue)
            {
                return ComparisonLower.Value * 2;
            }

            if (ComparisonUpper.HasValue)
            {
                return ComparisonUpper.Value / 2;
            }

            return 1.0;
        }
        
        
        private double? ComparisonLower { get; set; }
        private double? ComparisonUpper { get; set; }
        private string ComparisonType { get; set; }

        private void SetNextComparison()
        {
            var nextComparison = GetNextComparisonTask();

            nextComparison.MatchSome(c =>
            {
                Context.ComparisonTask = c;
            });
            
            nextComparison.MatchNone(() =>
            {
                PersistSelection();
                SetNextTask();
            });
        }

        private Option<Task.T> GetNextComparisonTask() 
            => _duty.api.comparisonOperand(Context.Task, ComparisonType, ComparisonLower, ComparisonUpper);

        private void TakeLeft()
        {
            var value = _duty.api.comparisonValue(Context.ComparisonTask, ComparisonType);
            ComparisonLower = value;

            SetNextComparison();
        }

        private void TakeRight()
        {
            var value = _duty.api.comparisonValue(Context.ComparisonTask, ComparisonType);
            ComparisonUpper = value;
            
            SetNextComparison();
        }

        private void OnLeft(object sender, ExecutedRoutedEventArgs e)
        {
            TakeLeft();
        }

        private void OnRight(object sender, ExecutedRoutedEventArgs e)
        {
            TakeRight();
        }

        private void PairwiseComparison_OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
            SetNextTask();
        }
    }
}