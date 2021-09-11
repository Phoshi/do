using System;
using System.Collections.Generic;
using Tasks;

namespace MarkdownSource
{
    public class TaskMetadata
    {
        public string Title { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public double? Importance { get; set; }
        public double? Urgency { get; set; }
        public double? Momentum { get; set; }
        public RelevanceRange Relevant { get; set; }
        public CycleRange.T Repeat { get; set; } = CycleRange.never;
        
        public IEnumerable<DateTime> Completed { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public IEnumerable<Confidence.T> Confidence { get; set; }
    }

    public class RelevanceRange
    {
        public DateTime? After { get; set; }
        public DateTime? Before { get; set; }
    }
}