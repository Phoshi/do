using System;
using System.Collections.Generic;
using System.Linq;
using Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Scalar = YamlDotNet.Core.Events.Scalar;

namespace MarkdownSource
{
    public class CycleRangeTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type.IsAssignableTo(typeof(CycleRange.T));
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            var times = new List<(string, string)>();
            if (parser.Current is Scalar never)
            {
                parser.MoveNext();
                return CycleRange.never;
            }

            parser.MoveNext();
            
            var name = parser.Consume<Scalar>().Value;
            
            var value = parser.Consume<Scalar>().Value;
            times.Add((name, value));
            if (parser.Current is Scalar)
            {
                var name2 = parser.Consume<Scalar>();
                var value2 = parser.Consume<Scalar>();
                times.Add((name2.Value, value2.Value));
            }
                
            parser.MoveNext();

            if (times.Count == 2)
            {
                return CycleRange.between(CycleTime("after"), CycleTime("before"));
            }

            if (times.Exists(kv => kv.Item1 == "after"))
            {
                return CycleRange.after(CycleTime("after"));
            }
            
            if (times.Exists(kv => kv.Item1 == "before"))
            {
                return CycleRange.before(CycleTime("before"));
            }

            return CycleRange.never;

            CycleRange.CycleTime.T CycleTime(string name)
                => Parse(times.First(t => t.Item1 == name).Item2);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var cycle = value as CycleRange.T;
            if (cycle == null || cycle.IsNever)
            {
                emitter.Emit(new Scalar("never"));
                return;
            }
                
            
            emitter.Emit(new MappingStart());

            if (cycle is CycleRange.T.After a)
            {
                emitter.Emit(new Scalar("after"));
                emitter.Emit(new Scalar(Stringify(a.Item)));
            }
            
            if (cycle is CycleRange.T.Before b)
            {
                emitter.Emit(new Scalar("before"));
                emitter.Emit(new Scalar(Stringify(b.Item)));
            }
            
            if (cycle is CycleRange.T.Between bet)
            {
                emitter.Emit(new Scalar("after"));
                emitter.Emit(new Scalar(Stringify(bet.Item.Item1)));
                
                emitter.Emit(new Scalar("before"));
                emitter.Emit(new Scalar(Stringify(bet.Item.Item2)));
            }
            
            emitter.Emit(new MappingEnd());
        }

        private CycleRange.CycleTime.T Parse(string timeDef)
            => CycleRange.CycleTime.parse(timeDef);

        private string Stringify(CycleRange.CycleTime.T cycle)
            => CycleRange.CycleTime.stringify(cycle);
    }
}