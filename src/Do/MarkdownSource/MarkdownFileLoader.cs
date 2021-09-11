using System;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownSource
{
    public class MarkdownFileLoader
    {
        private static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .Build();

        private static readonly IDeserializer Deserialiser =
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new ConfidenceTypeConverter())
                .WithTypeConverter(new CycleRangeTypeConverter())
                .IgnoreUnmatchedProperties()
                .Build();
        
        public Task.T Task(string pathRoot, string path, string title, DateTime created, string markdown)
        {
            var relativePath = Path.GetRelativePath(pathRoot, path);
            var doc = Markdown.Parse(markdown, Pipeline);

            var frontmatter = doc.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (frontmatter == null)
            {
                return Tasks.Task.createSimple(relativePath, title, markdown, created);
            }

            var yaml =
                frontmatter
                    .Lines
                    .Lines
                    .OrderByDescending(l => l.Line)
                    .Select(l => $"{l}\n")
                    .Select(l => l.Replace("---", ""))
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Aggregate((s, acc) => acc + s);

            var parsedYaml = Deserialiser.Deserialize<TaskMetadata>(yaml);

            var description = markdown.Split("---", 3, StringSplitOptions.RemoveEmptyEntries).Last().Trim();
            
            return Tasks.Task.create(
                relativePath,
                parsedYaml.Title ?? title,
                description,
                ListModule.OfSeq(parsedYaml.Tags ?? Enumerable.Empty<string>()),
                parsedYaml.Created ?? created,
                Importance(parsedYaml.Importance),
                Urgency(parsedYaml.Urgency),
                Relevance(parsedYaml.Relevant),
                parsedYaml.Repeat,
                ListModule.OfSeq(parsedYaml.Completed ?? Enumerable.Empty<DateTime>()),
                parsedYaml.LastUpdated ?? parsedYaml.Created ?? created,
                Momentum(parsedYaml.Momentum),
                ListModule.OfSeq(parsedYaml.Confidence ?? Enumerable.Empty<Confidence.T>())
            );
        }

        private Importance.T Importance(double? importance)
        {
            if (importance.HasValue)
                return Tasks.Importance.create(importance.Value);
            return Tasks.Importance.init;
        }
        
        private Urgency.T Urgency(double? urgency)
        {
            if (urgency.HasValue)
                return Tasks.Urgency.create(urgency.Value);
            return Tasks.Urgency.init;
        }
        
        private Momentum.T Momentum(double? momentum)
        {
            if (momentum.HasValue)
                return Tasks.Momentum.create(momentum.Value);
            return Tasks.Momentum.init;
        }

        private Tasks.RelevanceRange.T Relevance(RelevanceRange range)
        {
            if (range == null)
            {
                return Tasks.RelevanceRange.always;
            }

            if (!range.After.HasValue)
            {
                return Tasks.RelevanceRange.before(range.Before.Value);
            }

            if (!range.Before.HasValue)
            {
                return Tasks.RelevanceRange.after(range.After.Value);
            }

            return Tasks.RelevanceRange.between(range.Before.Value, range.After.Value);
        }
    }

    [TestFixture]
    public class GivenAMarkdownTask
    {
        private MarkdownFileLoader Loader()
        {
            return new MarkdownFileLoader();
        }
        [Test]
        public void SimpleTasksParse()
        {
            var task = Loader().Task("", "file.md", "Test task", new DateTime(2020, 5, 5), "Simple task!");

            Assert.That(task,
                Is.EqualTo(
                    Tasks.Task.createSimple(
                        "file.md",
                        "Test task", 
                        "Simple task!", 
                        new DateTime(2020, 5, 5))));
        }
        
        [Test]
        public void ComplexTasksParse()
        {
            var task = Loader().Task(
                "",
                "file.md",
                "Test task", 
                new DateTime(2020, 5, 5), 
                @"---
created: 2020-05-06
relevant: 
  after: 2020-06-01
momentum: 3
importance: 0.8
urgency: 3
---

Simple task!");

            Assert.That(task,
                Is.EqualTo(
                    Tasks.Task.create(
                        "file.md",
                        "Test task",
                        "Simple task!",
                        FSharpList<string>.Empty,
                        new DateTime(2020, 5, 6),
                        Importance.create(0.8),
                        Urgency.create(3),
                        Tasks.RelevanceRange.after(new DateTime(2020, 6, 1)),
                        Tasks.CycleRange.never,
                        FSharpList<DateTime>.Empty,
                        new DateTime(2020, 5, 6),
                        Momentum.create(3),
                        FSharpList<Confidence.T>.Empty)));
        }
        
        [Test]
        public void TasksWithBeforeCyclesParse()
        {
            var task = Loader().Task(
                "",
                "file.md",
                "Test task", 
                new DateTime(2020, 5, 5), 
                @"---
created: 2020-05-06
relevant: 
  after: 2020-06-01
repeat:
  before: 3 weeks
momentum: 3
importance: 0.8
urgency: 3
---

Simple task!");

            Assert.That(task,
                Is.EqualTo(
                    Tasks.Task.create(
                        "file.md",
                        "Test task",
                        "Simple task!",
                        FSharpList<string>.Empty,
                        new DateTime(2020, 5, 6),
                        Importance.create(0.8),
                        Urgency.create(3),
                        Tasks.RelevanceRange.after(new DateTime(2020, 6, 1)),
                        Tasks.CycleRange.before(
                            CycleRange.CycleTime.create(0, 0, 3, 0, 0, 0)),
                        FSharpList<DateTime>.Empty,
                        new DateTime(2020, 5, 6),
                        Momentum.create(3),
                        FSharpList<Confidence.T>.Empty)));
        }
        [Test]
        public void TasksWithCyclesParse()
        {
            var task = Loader().Task(
                "",
                "file.md",
                "Test task", 
                new DateTime(2020, 5, 5), 
                @"---
created: 2020-05-06
relevant: 
  after: 2020-06-01
repeat:
  before: 3 weeks
  after: 1 week, 2 days
momentum: 3
importance: 0.8
urgency: 3
---

Simple task!");

            Assert.That(task,
                Is.EqualTo(
                    Tasks.Task.create(
                        "file.md",
                        "Test task",
                        "Simple task!",
                        FSharpList<string>.Empty,
                        new DateTime(2020, 5, 6),
                        Importance.create(0.8),
                        Urgency.create(3),
                        Tasks.RelevanceRange.after(new DateTime(2020, 6, 1)),
                        Tasks.CycleRange.between(
                            CycleRange.CycleTime.create(0, 0, 1, 2, 0, 0),
                            CycleRange.CycleTime.create(0, 0, 3, 0, 0, 0)),
                        FSharpList<DateTime>.Empty,
                        new DateTime(2020, 5, 6),
                        Momentum.create(3),
                        FSharpList<Confidence.T>.Empty)));
        }
        
    }
}