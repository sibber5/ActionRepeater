using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using System.Text.Json;

namespace ActionRepeater.Core.Helpers;

public sealed class ActionData
{
    public IReadOnlyList<InputAction>? Actions { get; init; }

    public MouseMovement? CursorPathStartAbs { get; init; }

    public IReadOnlyList<MouseMovement>? CursorPathRel { get; init; }
}

public static class SerializationHelper
{
    private static JsonSerializerOptions _serializerOptions
    {
        get
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                IncludeFields = true
            };
            options.Converters.Add(new InputActionJsonConverter());
            return options;
        }
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task SerializeActionsAsync(ActionData obj, string path)
    {
        using FileStream createStream = File.Create(path);

        await JsonSerializer.SerializeAsync(createStream, obj, _serializerOptions);

        await createStream.DisposeAsync();
    }

    public static async Task<ActionData?> DeserializeActionsAsync(string path)
    {
        using FileStream openStream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<ActionData?>(openStream, _serializerOptions);
    }
}
