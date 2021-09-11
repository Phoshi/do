using System.Collections.Generic;
using System.Xml;
using Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownSource
{
    public class MetadataSource
    {
        private static readonly IDeserializer Deserialiser =
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new ConfidenceTypeConverter())
                .WithTypeConverter(new CycleRangeTypeConverter())
                .IgnoreUnmatchedProperties()
                .Build();

        public T Metadata<T>(string meta)
        {
            return Deserialiser.Deserialize<T>(meta);
        }
    }

    public class Metadata
    {
        public string name { get; set; }
        public ReviewMeta review { get; set; }
        public Strings strings { get; set; }
        public IEnumerable<CaptureMeta> capture { get; set; }
        public IEnumerable<string> measures { get; set; }
        public Dictionary<string, TagOptions> tags { get; set; }
    }

    public class CaptureMeta
    {
        public string key { get; set; }
        public string description { get; set; }
        
        public bool captureTask { get; set; }
        public bool beginReview { get; set; }
        public string scriptPath { get; set; }
    }

    public class ReviewMeta
    {
        public int size { get; set; }
        public int assignments { get; set; }
        public string frequency { get; set; }
    }

    public class Strings {
        public string assigned { get; set; }
        public string done { get; set; }
    }

    public class TagOptions
    {
        public IEnumerable<string> measures { get; set; }
        public bool @override { get; set; }
        
        public ReviewMeta review { get; set; }
    }
}