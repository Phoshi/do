using System;
using System.ComponentModel;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using GuidConverter = YamlDotNet.Serialization.Converters.GuidConverter;

namespace MarkdownSource
{
    public class MarkdownFileWriter
    {
        private static readonly ISerializer Serializer =
            new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new ConfidenceTypeConverter())
                .WithTypeConverter(new CycleRangeTypeConverter())
                .WithEmissionPhaseObjectGraphVisitor(args => new SmartDefaultExclusiveObjectGraphVisitor(args.InnerVisitor))
                .Build();
        
        public string Write(Task.T task)
        {
            return $@"{Frontmatter(task)}{task.description}";
        }

        private string Frontmatter(Task.T task)
        {
            var meta = new TaskMetadata
            {
                Completed = task.completed,
                Created = task.created,
                LastUpdated = task.lastUpdated,
                Importance = task.importance.Item,
                Momentum = task.momentum.Item,
                Relevant = RelevanceRange(task.relevanceRange),
                Repeat = task.cycleRange,
                Tags = task.tags,
                Urgency = task.urgency.Item,
                Confidence = task.confidence
            };

            var yaml = Serializer.Serialize(meta);
            return $@"---
{yaml}
---

";
        }

        private RelevanceRange RelevanceRange(Tasks.RelevanceRange.T range)
        {
            if (range is Tasks.RelevanceRange.T.After a)
            {
                return new RelevanceRange
                {
                    After = a.Item
                };
            }
            else if (range is Tasks.RelevanceRange.T.Before b)
            {
                return new RelevanceRange
                {
                    Before = b.Item
                };
            }
            else if (range is Tasks.RelevanceRange.T.Between be)
            {
                return new RelevanceRange
                {
                    Before = be.Item.Item1,
                    After = be.Item.Item2,
                };
            }
            else
            {
                return null;
            }
        }
        
    }

    [TestFixture]
    public class GivenAnUpdatedTask
    {
        private MarkdownFileWriter Writer => new MarkdownFileWriter();

        [Test]
        public void ThePresentValuesAreWrittenToFrontmatter()
        {
            var task =
                Tasks.Task.setCycleRange(Tasks.CycleRange.before(CycleRange.CycleTime.fromTimeSpan(TimeSpan.FromDays(7))),
                    Tasks.Task.createSimple("file.md", "hello", "world", new DateTime(2020, 5, 5)));

            var serialised = Writer.Write(task);
            
            Assert.That(serialised, Is.EqualTo(@"---
repeat:
  before: 7 days
created: 2020-05-05T00:00:00.0000000
lastUpdated: 2020-05-05T00:00:00.0000000

---

world"));
        }
    }
    
        public sealed class SmartDefaultExclusiveObjectGraphVisitor : ChainedObjectGraphVisitor
        {
            public SmartDefaultExclusiveObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
                : base(nextVisitor)
            {
            }
    
            private static object? GetDefault(Type type)
            {
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }
    
            public override bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, IEmitter context)
            {
                return !Equals(value.Value, GetDefault(value.Type))
                       && base.EnterMapping(key, value, context);
            }
    
            public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
            {
                var defaultValueAttribute = key.GetCustomAttribute<DefaultValueAttribute>();
                var defaultValue = defaultValueAttribute != null
                    ? defaultValueAttribute.Value
                    : GetDefault(key.Type);

                if (value.Value is FSharpList<string> fsl && fsl.Length == 0)
                {
                    return false;
                }
                if (value.Value is FSharpList<DateTime> dtl && dtl.Length == 0)
                {
                    return false;
                }
                if (value.Value is FSharpList<Confidence.T> cl && cl.Length == 0)
                {
                    return false;
                }

                if (value.Value is double d && d == 1.0)
                {
                    return false;
                }
    
                return !Equals(value.Value, defaultValue)
                       && base.EnterMapping(key, value, context);
            }
        }
}