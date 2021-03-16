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

        public Metadata Metadata(string meta)
        {
            return Deserialiser.Deserialize<Metadata>(meta);
        }
    }

    public class Metadata
    {
        public string name { get; set; }
        public ReviewMeta review { get; set; }
    }

    public class ReviewMeta
    {
        public int size { get; set; }
        public int assignments { get; set; }
    }

}