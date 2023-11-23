namespace AnmTool;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpNeedle.LostWorld.Animation;

#pragma warning disable IL2026
#pragma warning disable IL3050

[JsonSerializable(typeof(ProxyCharAnimScript))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, UseStringEnumConverter = true)]
internal partial class JsonTypeContext : JsonSerializerContext
{
    public static readonly JsonSerializerOptions SerializerOptions;

    static JsonTypeContext()
    {
        SerializerOptions = new()
        {
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true,
            TypeInfoResolver = Default,
            WriteIndented = true,
            IncludeFields = true,
            Converters =
            {
                new CallbackParamConverter(), 
                new ComplexDataConverter(), 
                new ComplexAnimationConverter(),
                new SequenceTableConverter()
            }
        };
    }
}

public class CallbackParamConverter : JsonConverter<TriggerInfo.CallbackParam>
{
    public override TriggerInfo.CallbackParam Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    => reader.TokenType switch
    {
        JsonTokenType.String => new TriggerInfo.CallbackParam(reader.GetString()),
        JsonTokenType.Number when reader.TryGetInt32(out var i) => new TriggerInfo.CallbackParam(i) { Type = TriggerValueType.Enum },
        JsonTokenType.Number => new TriggerInfo.CallbackParam(reader.GetSingle()),
        _ => throw new NotSupportedException("Invalid callback parameter type"),
    };

    public override void Write(Utf8JsonWriter writer, TriggerInfo.CallbackParam value, JsonSerializerOptions options)
    {
        switch (value.Type)
        {
            case TriggerValueType.String:
                writer.WriteStringValue(value.String);
                break;

            case TriggerValueType.Float:
                writer.WriteNumberValue(value.Float);
                break;

            default:
                writer.WriteNumberValue(value.Integer);
                break;
        }
    }
}

public class ComplexAnimationConverter : JsonConverter<ComplexAnimation>
{
    public override ComplexAnimation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var animation = new ComplexAnimation();
        
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            reader.Read();
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString()!;
                if (name == nameof(ComplexAnimation.Name))
                {
                    reader.Read();
                    animation.Name = reader.GetString();
                }
                else if (name == nameof(ComplexAnimation.Layer))
                {
                    reader.Read();
                    animation.Layer = reader.GetInt16();
                }
                else if (name == nameof(ComplexAnimation.Animations))
                {
                    animation.Animations = JsonSerializer.Deserialize<List<SimpleAnimation>>(ref reader, options);
                }
                else if (name == nameof(ComplexAnimation.Data))
                {
                    animation.Data = JsonSerializer.Deserialize<IComplexData>(ref reader, options);
                }
            }
        }

        reader.Read();
        return animation;
    }

    public override void Write(Utf8JsonWriter writer, ComplexAnimation value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString(nameof(ComplexAnimation.Name), value.Name);
        writer.WriteNumber(nameof(ComplexAnimation.Layer), value.Layer);
        
        writer.WriteStartArray(nameof(ComplexAnimation.Animations));

        foreach (var animation in value.Animations)
        {
            JsonSerializer.Serialize(writer, animation, options);
        }

        writer.WriteEndArray();
        
        writer.WritePropertyName(nameof(ComplexAnimation.Data));
        JsonSerializer.Serialize(writer, value.Data, options);

        writer.WriteEndObject();
    }
}

public class ComplexDataConverter : JsonConverter<IComplexData>
{
    public override IComplexData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Discard start of object
        reader.Read();
        reader.Read();

        var type = reader.GetString()!.ToLower();
        reader.Read();

        if (type == "blender")
        {
            var data = JsonSerializer.Deserialize<BlenderData>(ref reader, options);
            reader.Read();
            return data;
        }
        else if (type == "sequence")
        {
            var data = JsonSerializer.Deserialize<SequenceTable>(ref reader, options);
            reader.Read();
            return data;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, IComplexData value, JsonSerializerOptions options)
    {
        if (value is BlenderData blender)
        {
            writer.WriteStartObject();
            writer.WriteString("Type", "Blender");
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, blender, options);
            writer.WriteEndObject();
        }
        else if (value is SequenceTable seqTable)
        {
            writer.WriteStartObject();
            writer.WriteString("Type", "Sequence");
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, seqTable, options);
            writer.WriteEndObject();
        }
    }
}

public class SequenceTableConverter : JsonConverter<SequenceTable>
{
    public override SequenceTable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var table = new SequenceTable();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var property = reader.GetString();
                reader.Read();

                if (property == nameof(SequenceTable.PlayMode))
                {
                    table.PlayMode = JsonSerializer.Deserialize<PlayModeInfo>(ref reader, options);
                    reader.Read();
                    continue;
                }
                else if (property == "Animations")
                {
                    reader.Read();
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        table.Add(reader.GetString());
                        reader.Read();
                    }
                }
            }

            reader.Read();
        }

        reader.Read();
        return table;
    }

    public override void Write(Utf8JsonWriter writer, SequenceTable value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(SequenceTable.PlayMode));
        JsonSerializer.Serialize(writer, value.PlayMode, options);

        writer.WriteStartArray("Animations");

        foreach (var animation in value)
        {
            writer.WriteStringValue(animation);
        }
        
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}