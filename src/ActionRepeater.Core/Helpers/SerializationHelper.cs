using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using System.Text.Json;

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
            options.Converters.Add(new InputActionJsonConverter());
            return options;
        }
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task SerializeAsync<T>(T obj, string path)
    {
        await using FileStream createStream = File.Create(path);

        await JsonSerializer.SerializeAsync(createStream, obj, _baseSerializerOptions);
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task<T?> DeserializeAsync<T>(string path)
    {
        await using FileStream openStream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<T>(openStream, _baseSerializerOptions);
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task SerializeActionsAsync(ActionData obj, string path)
    {
        await using FileStream createStream = File.Create(path);

        await JsonSerializer.SerializeAsync(createStream, obj, _actionDataSerializerOptions);
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static async Task<ActionData?> DeserializeActionsAsync(string path)
    {
        await using FileStream openStream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<ActionData?>(openStream, _actionDataSerializerOptions);
    }
}
