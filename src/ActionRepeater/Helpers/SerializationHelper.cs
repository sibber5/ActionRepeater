using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;
using ActionRepeater.Action;

namespace ActionRepeater.Helpers;

public static class SerializationHelper
{
    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static void Serialize<T>(in T obj, string path)
    {
        XmlSerializer serializer = new(typeof(T));
        var writer = new StreamWriter(path);
        var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ", // 4 spaces
            CloseOutput = true,
            NewLineHandling = NewLineHandling.Replace,
            Encoding = System.Text.Encoding.UTF8
        });

        try
        {
            serializer.Serialize(xmlWriter, obj, new XmlSerializerNamespaces(new XmlQualifiedName[] { new(string.Empty) }));
        }
        finally
        {
            xmlWriter.Close();
            xmlWriter.Dispose();
            writer.Dispose();
        }
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static T? Deserialize<T>(string path)
    {
        FileStream stream = new(path, FileMode.Open);
        XmlSerializer serializer = new(typeof(T));

        T? loadedObj;
        try
        {
            loadedObj = (T?)serializer.Deserialize(stream);
        }
        finally
        {
            stream.Close();
        }

        return loadedObj;
    }

    public static async Task DeserializeActionsAsync(System.Collections.ObjectModel.ObservableCollection<InputAction> actions, string path)
    {
        FileStream stream = new(path, FileMode.Open);

        using XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings()
        {
            Async = true,
            CloseInput = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        });

        await reader.MoveToContentAsync();

        if (!reader.Name.Equals($"ArrayOf{nameof(InputAction)}", StringComparison.Ordinal))
        {
            throw new FormatException($"Invalid XML element \"{reader.Name}\". Expected \"ArrayOf{nameof(InputAction)}\".");
        }

        // do...while instead of regular while because MoveToContentAsync() was called earlier
        do
        {
            if (reader.NodeType != XmlNodeType.Element) continue;

            if (!reader.Name.Equals(nameof(InputAction), StringComparison.Ordinal)) continue;

            string? actiontype = reader.GetAttribute("Type");

            if (reader.AttributeCount != 1 || actiontype is null)
            {
                throw new FormatException($"Invalid XML attributes for element \"{nameof(InputAction)}\". Expected 1 attribute \"Type\".");
            }

            actions.Add(actiontype switch
            {
                nameof(KeyAction) => await KeyAction.CreateActionFromXmlAsync(reader),
                nameof(MouseButtonAction) => await MouseButtonAction.CreateActionFromXmlAsync(reader),
                nameof(MouseWheelAction) => await MouseWheelAction.CreateActionFromXmlAsync(reader),
                nameof(WaitAction) => await WaitAction.CreateActionFromXmlAsync(reader),
                _ => throw new FormatException($"Unexpected XML attribute value \"{actiontype}\".")
            });
        } while (await reader.ReadAsync());
    }
}
