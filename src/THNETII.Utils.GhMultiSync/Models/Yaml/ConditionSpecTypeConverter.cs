using System;
using System.Collections.Generic;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync.Models.Yaml
{
    public class ConditionSpecTypeConverter : IYamlTypeConverter
    {
        private readonly Lazy<IDeserializer> deserializer;
        private readonly Func<IDeserializer> deserializerAccessor;

        private IDeserializer Deserializer => deserializer.Value;

        public ConditionSpecTypeConverter(Func<IDeserializer> deserializerAccessor) : base()
        {
            this.deserializerAccessor = deserializerAccessor;
            deserializer = new Lazy<IDeserializer>(GetDeserializer);
        }

        private IDeserializer GetDeserializer()
        {
            return deserializerAccessor?.Invoke() ??
                new DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .WithTypeConverter(this)
                    .Build();
        }

        public bool Accepts(Type type) => type == typeof(ConditionSpec);

        public object ReadYaml(IParser parser, Type type)
        {
            if (!Accepts(type))
                throw new InvalidOperationException($"Cannot convert YAML into type {type}");
            if (parser is null)
                return null;

            var root = parser.Current;
            switch (root)
            {
                case Scalar scalar:
                    if (!parser.MoveNext())
                        throw new InvalidOperationException($"Unexpected end of parser encountered! {parser.Current}");
                    return ReadInnerYaml(parser, scalar.Value, hasValue: false);
                case SequenceStart _:
                    var conditions = (List<ConditionSpec>)Deserializer.Deserialize(parser, typeof(List<ConditionSpec>));
                    return new ConditionAndSpec { Conditions = conditions };
                case MappingStart _:
                    conditions = new List<ConditionSpec>();
                    while (parser.MoveNext() && !(parser.Current is MappingEnd))
                    {
                        var scalar = parser.Expect<Scalar>();
                        conditions.Add(ReadInnerYaml(parser, scalar.Value, true));
                    }
                    if (!parser.MoveNext())
                        throw new InvalidOperationException($"Unexpected end of parser encountered! {parser.Current}");
                    switch (conditions.Count)
                    {
                        case 0:
                            return null;
                        case 1:
                            return conditions[0];
                        default:
                            return new ConditionAndSpec { Conditions = conditions };
                    }
            }

            throw new InvalidOperationException($"Encountered unexpected parsing event of type {root.GetType()}. {root}");
        }

        private ConditionSpec ReadInnerYaml(IParser parser, string name, bool hasValue)
        {
            switch (name)
            {
                case null:
                    throw new ArgumentNullException(nameof(name));

                case string _ when "not".Equals(name, StringComparison.OrdinalIgnoreCase):
                    if (!hasValue)
                        return null;
                    switch (parser.Current)
                    {
                        case Scalar scalar:
                            return new ConditionNotSpec { Condition = ReadInnerYaml(parser, scalar.Value, hasValue: false) };
                        case SequenceStart _:
                            var conditions = (List<ConditionSpec>)Deserializer.Deserialize(parser, typeof(List<ConditionSpec>));
                            return new ConditionNotSpec { Condition = new ConditionAndSpec { Conditions = conditions } };
                        case MappingStart _:
                            return Deserializer.Deserialize<ConditionNotSpec>(parser);
                    }
                    break;

                case string _ when "destinationExists".Equals(name, StringComparison.OrdinalIgnoreCase):
                    if (hasValue)
                        parser.SkipThisAndNestedEvents();
                    return new ConditionDestinationExistsSpec();
                
                default:
                    throw new InvalidOperationException($"Unable to convert condition spec with name: {name}");
            }

            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
