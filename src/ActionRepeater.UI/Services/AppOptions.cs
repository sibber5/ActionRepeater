using System.Text.Json.Serialization;
using ActionRepeater.Core;

namespace ActionRepeater.UI.Services;

public record AppOptions(CoreOptions Core, UIOptions UI);

[JsonSerializable(typeof(AppOptions))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, WriteIndented = true)]
public partial class AppOptionsJsonContext : JsonSerializerContext { }
