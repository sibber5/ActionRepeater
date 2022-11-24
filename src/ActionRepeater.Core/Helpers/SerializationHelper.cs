using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core.Helpers;

public sealed record ActionData(IReadOnlyList<InputAction>? Actions, MouseMovement? CursorPathStartAbs, IReadOnlyList<MouseMovement>? CursorPathRelative);

public static class SerializationHelper
{
    private static JsonSerializerOptions _baseSerializerOptions => new()
    {
        WriteIndented = true,
    };

    private static JsonSerializerOptions _actionDataSerializerOptions
    {
        get
        {
            JsonSerializerOptions options = _baseSerializerOptions;
            options.IncludeFields = true;
            return options;
        }
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task SerializeToFileAsync<T>(T obj, string path)
    {
        await using FileStream createStream = File.Create(path);

        await JsonSerializer.SerializeAsync(createStream, obj, obj is ActionData ? _actionDataSerializerOptions : _baseSerializerOptions);
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task<T?> DeserializeFromFileAsync<T>(string path)
    {
        await using FileStream openStream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<T>(openStream, typeof(T) == typeof(ActionData) ? _actionDataSerializerOptions : _baseSerializerOptions);
    }
}
