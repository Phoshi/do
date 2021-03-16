using System;
using Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Scalar = YamlDotNet.Core.Events.Scalar;

namespace MarkdownSource
{
    public class ConfidenceTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(Confidence.T);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            parser.MoveNext();
            var name = parser.Consume<Scalar>();
            var value = parser.Consume<Scalar>();
            parser.MoveNext();

            return Confidence.create(name.Value, double.Parse(value.Value));
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var confidence = value as Confidence.T;
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(confidence.measure));
            emitter.Emit(new Scalar(confidence.confidence.ToString()));
            emitter.Emit(new MappingEnd());
        }
    }
}