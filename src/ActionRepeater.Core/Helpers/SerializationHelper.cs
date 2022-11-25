using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core.Helpers;

public sealed record ActionData(IReadOnlyList<InputAction>? Actions, MouseMovement? CursorPathStartAbs, IReadOnlyList<MouseMovement>? CursorPathRelative);

[JsonSerializable(typeof(ActionData))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, IncludeFields = true, WriteIndented = true)]
internal partial class ActionDataJsonContext : JsonSerializerContext { }

public static class SerializationHelper
{
    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task SerializeToFileAsync<T>(T obj, string path, JsonTypeInfo<T> typeInfo)
    {
        await using FileStream createStream = File.Create(path);

        await JsonSerializer.SerializeAsync(createStream, obj, typeInfo);
    }

    /// <inheritdoc cref="SerializeToFileAsync{T}(T, string, JsonTypeInfo{T})"/>
    public static async Task<T?> DeserializeFromFileAsync<T>(string path, JsonTypeInfo<T> typeInfo)
    {
        await using FileStream openStream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<T>(openStream, typeInfo);
    }

    /// <inheritdoc cref="SerializeToFileAsync{T}(T, string, JsonTypeInfo{T})"/>
    public static Task SerializeActionsToFileAsync(ActionData obj, string path) => SerializeToFileAsync(obj, path, ActionDataJsonContext.Default.ActionData);

    /// <inheritdoc cref="SerializeToFileAsync{T}(T, string, JsonTypeInfo{T})"/>
    public static Task<ActionData?> DeserializeActionsFromFileAsync(string path) => DeserializeFromFileAsync(path, ActionDataJsonContext.Default.ActionData);
}
