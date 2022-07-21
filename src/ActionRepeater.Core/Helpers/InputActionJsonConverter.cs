using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core.Helpers;

internal sealed class InputActionJsonConverter : JsonConverter<InputAction>
{
    private const string _posX = "PositionX";
    private const string _posY = "PositionY";
    private const string _typeDiscriminatorName = "$type";

    private static string GetPropertyNameExceptionMessage(string actionTypeName, string? propertyName)
        => $"Unexpected property name while deserializing {actionTypeName}: {propertyName}.";

    private static string GetPropertyNameExceptionMessage(string actionTypeName, string? propertyName, string? expectedName)
        => $"Unexpected property name while deserializing {actionTypeName}: {propertyName}. Expected: {expectedName}";

    private static string GetTokenTypeExceptionMessage(string actionTypeName, JsonTokenType tokenType, JsonTokenType expectedTokenType)
        => $"Unexpected json token type while deserializing {actionTypeName}: {tokenType}. Expected: {expectedTokenType}";

    private static string GetPropertyCountExceptionMessage(string actionTypeName, int readPropertyCount, int expectedPropertyCount)
        => $"Unexpected number of properties for {actionTypeName}: {readPropertyCount}. Expected {expectedPropertyCount}";

    private enum TypeDiscriminator
    {
        KeyAction = 1,
        MouseButtonAction = 2,
        MouseWheelAction = 3,
        WaitAction = 4,
    }

    public override bool CanConvert(Type typeToConvert) => typeof(InputAction).IsAssignableFrom(typeToConvert);

    public override InputAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(GetTokenTypeExceptionMessage(nameof(InputAction), reader.TokenType, JsonTokenType.StartObject));
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException(GetTokenTypeExceptionMessage(nameof(InputAction), reader.TokenType, JsonTokenType.PropertyName));
        }

        string? propertyName = reader.GetString();
        if (propertyName != _typeDiscriminatorName)
        {
            throw new JsonException(GetPropertyNameExceptionMessage(nameof(InputAction), propertyName, _typeDiscriminatorName));
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException(GetTokenTypeExceptionMessage(nameof(InputAction), reader.TokenType, JsonTokenType.Number));
        }

        TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
        return typeDiscriminator switch
        {
            TypeDiscriminator.KeyAction => DeserializeKeyAction(ref reader),
            TypeDiscriminator.MouseButtonAction => DeserializeMouseButtonAction(ref reader),
            TypeDiscriminator.MouseWheelAction => DeserializeMouseWheelAction(ref reader),
            TypeDiscriminator.WaitAction => DeserializeWaitAction(ref reader),
            _ => throw new JsonException($"No type found for corresponding TypeDiscriminator: {typeDiscriminator}"),
        };
    }

    private KeyAction DeserializeKeyAction(ref Utf8JsonReader reader)
    {
        const int keyActionPropertyCount = 3;

        KeyActionType actionType = default;
        Win32.Input.VirtualKey key = default;
        bool isAutoRepeat = default;

        int readPropertiesCount = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(KeyAction.ActionType):
                        actionType = (KeyActionType)reader.GetInt32();
                        break;

                    case nameof(KeyAction.Key):
                        key = (Win32.Input.VirtualKey)reader.GetInt32();
                        break;

                    case nameof(KeyAction.IsAutoRepeat):
                        isAutoRepeat = reader.GetBoolean();
                        break;

                    default:
                        throw new JsonException(GetPropertyNameExceptionMessage(nameof(KeyAction), propertyName));
                }

                ++readPropertiesCount;
            }
            else
            {
                throw new JsonException(GetTokenTypeExceptionMessage(nameof(KeyAction), reader.TokenType, JsonTokenType.PropertyName));
            }
        }

        if (readPropertiesCount != keyActionPropertyCount)
        {
            throw new JsonException(GetPropertyCountExceptionMessage(nameof(KeyAction), readPropertiesCount, keyActionPropertyCount));
        }

        return new(actionType, key, isAutoRepeat);
    }

    private MouseButtonAction DeserializeMouseButtonAction(ref Utf8JsonReader reader)
    {
        const int mouseButtonActionPropertyCount = 5;

        MouseButtonActionType actionType = default;
        Input.InputSimulator.MouseButton button = default;
        int posX = default;
        int posY = default;
        bool usePosition = default;

        int readPropertiesCount = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(MouseButtonAction.ActionType):
                        actionType = (MouseButtonActionType)reader.GetInt32();
                        break;

                    case nameof(MouseButtonAction.Button):
                        button = (Input.InputSimulator.MouseButton)reader.GetInt32();
                        break;

                    case _posX:
                        posX = reader.GetInt32();
                        break;

                    case _posY:
                        posY = reader.GetInt32();
                        break;

                    case nameof(MouseButtonAction.UsePosition):
                        usePosition = reader.GetBoolean();
                        break;

                    default:
                        throw new JsonException(GetPropertyNameExceptionMessage(nameof(MouseButtonAction), propertyName));
                }

                ++readPropertiesCount;
            }
            else
            {
                throw new JsonException(GetTokenTypeExceptionMessage(nameof(MouseButtonAction), reader.TokenType, JsonTokenType.PropertyName));
            }
        }

        if (readPropertiesCount != mouseButtonActionPropertyCount)
        {
            throw new JsonException(GetPropertyCountExceptionMessage(nameof(MouseButtonAction), readPropertiesCount, mouseButtonActionPropertyCount));
        }

        return new(actionType, button, new Win32.POINT(posX, posY), usePosition);
    }

    private MouseWheelAction DeserializeMouseWheelAction(ref Utf8JsonReader reader)
    {
        const int keyActionPropertyCount = 3;

        bool isHorizontal = default;
        int stepCount = default;
        int duration = default;

        int readPropertiesCount = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(MouseWheelAction.IsHorizontal):
                        isHorizontal = reader.GetBoolean();
                        break;

                    case nameof(MouseWheelAction.StepCount):
                        stepCount = reader.GetInt32();
                        break;

                    case nameof(MouseWheelAction.Duration):
                        duration = reader.GetInt32();
                        break;

                    default:
                        throw new JsonException(GetPropertyNameExceptionMessage(nameof(MouseWheelAction), propertyName));
                }

                ++readPropertiesCount;
            }
            else
            {
                throw new JsonException(GetTokenTypeExceptionMessage(nameof(MouseWheelAction), reader.TokenType, JsonTokenType.PropertyName));
            }
        }

        if (readPropertiesCount != keyActionPropertyCount)
        {
            throw new JsonException(GetPropertyCountExceptionMessage(nameof(MouseWheelAction), readPropertiesCount, keyActionPropertyCount));
        }

        return new(isHorizontal, stepCount, duration);
    }

    private WaitAction DeserializeWaitAction(ref Utf8JsonReader reader)
    {
        const int waitActionPropertyCount = 1;

        int duration = default;

        int readPropertiesCount = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                reader.Read();

                if (propertyName == nameof(WaitAction.Duration))
                {
                    duration = reader.GetInt32();
                }
                else
                {
                    throw new JsonException(GetPropertyNameExceptionMessage(nameof(WaitAction), propertyName));
                }

                ++readPropertiesCount;
            }
            else
            {
                throw new JsonException(GetTokenTypeExceptionMessage(nameof(WaitAction), reader.TokenType, JsonTokenType.PropertyName));
            }
        }

        if (readPropertiesCount != waitActionPropertyCount)
        {
            throw new JsonException(GetPropertyCountExceptionMessage(nameof(WaitAction), readPropertiesCount, waitActionPropertyCount));
        }

        return new(duration);
    }

    public override void Write(Utf8JsonWriter writer, InputAction value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case KeyAction keyAction:
                writer.WriteNumber(_typeDiscriminatorName, (int)TypeDiscriminator.KeyAction);
                writer.WriteNumber(nameof(keyAction.ActionType), (int)keyAction.ActionType);
                writer.WriteNumber(nameof(keyAction.Key), (int)keyAction.Key);
                writer.WriteBoolean(nameof(keyAction.IsAutoRepeat), keyAction.IsAutoRepeat);
                break;

            case MouseButtonAction mbAction:
                writer.WriteNumber(_typeDiscriminatorName, (int)TypeDiscriminator.MouseButtonAction);
                writer.WriteNumber(nameof(mbAction.ActionType), (int)mbAction.ActionType);
                writer.WriteNumber(nameof(mbAction.Button), (int)mbAction.Button);
                writer.WriteNumber(_posX, mbAction.Position.x);
                writer.WriteNumber(_posY, mbAction.Position.y);
                writer.WriteBoolean(nameof(mbAction.UsePosition), mbAction.UsePosition);
                break;

            case MouseWheelAction mwAction:
                writer.WriteNumber(_typeDiscriminatorName, (int)TypeDiscriminator.MouseWheelAction);
                writer.WriteBoolean(nameof(mwAction.IsHorizontal), mwAction.IsHorizontal);
                writer.WriteNumber(nameof(mwAction.StepCount), mwAction.StepCount);
                writer.WriteNumber(nameof(mwAction.Duration), mwAction.Duration);
                break;

            case WaitAction waitAction:
                writer.WriteNumber(_typeDiscriminatorName, (int)TypeDiscriminator.WaitAction);
                writer.WriteNumber(nameof(waitAction.Duration), waitAction.Duration);
                break;
        }

        writer.WriteEndObject();
    }
}
