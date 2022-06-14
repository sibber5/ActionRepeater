using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;

namespace ActionRepeater.Core.Helpers;

public sealed class ActionData
{
    public System.Collections.ObjectModel.Collection<InputAction>? Actions { get; init; }

    public MouseMovement? CursorPathStartAbs { get; init; }

    public List<MouseMovement>? CursorPathRel { get; init; }
}

public static class SerializationHelper
{
    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static void Serialize(ActionData obj, string path)
    {
        // TODO: not all [public] properties are serialized in KeyAction

        XmlSerializer serializer = new(typeof(ActionData));
        var writer = new StreamWriter(path);
        var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ", // 4 spaces
            CloseOutput = true,
            NewLineHandling = NewLineHandling.Replace,
            Encoding = System.Text.Encoding.UTF8,
            OmitXmlDeclaration = true
        });

        try
        {
            serializer.Serialize(xmlWriter, obj/*, new XmlSerializerNamespaces(new XmlQualifiedName[] { new(null) })*/);
        }
        finally
        {
            xmlWriter.Close();
            xmlWriter.Dispose();
            writer.Dispose();
        }
    }

    /// <param name="path">The full path of the file, including its name and extention.</param>
    public static ActionData? Deserialize(string path)
    {
        FileStream stream = new(path, FileMode.Open);
        XmlSerializer serializer = new(typeof(ActionData));

        ActionData? loadedObj;
        try
        {
            loadedObj = (ActionData?)serializer.Deserialize(stream);
        }
        finally
        {
            stream.Close();
        }

        return loadedObj;
    }

    /// <returns>null if deserialization was successful, otherwise returns the error message.</returns>
    public static async Task<string?> DeserializeActionsAsync(string path)
    {
        ActionData? dat = null;

        try
        {
            await Task.Run(() => dat = Deserialize(path));
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }

        if (dat is null) return null;

        Input.ActionManager.ClearAll();

        if (dat.Actions is not null)
        {
            for (int i = 0; i < dat.Actions.Count; ++i)
            {
                Input.ActionManager.Actions.Add(dat.Actions[i]);
            }
        }

        if (dat.CursorPathRel is not null)
        {
            if (dat.CursorPathStartAbs is null) throw new InvalidOperationException($"There is not start position ({nameof(dat.CursorPathStartAbs)} is null), but the cursor path is not empty.");

            Input.ActionManager.CursorPathStart = dat.CursorPathStartAbs;
            Input.ActionManager.CursorPath.AddRange(dat.CursorPathRel);
        }
        else if (dat.CursorPathStartAbs is not null)
        {
            throw new InvalidOperationException($"The cursor path is not empty, but there is a start position ({nameof(dat.CursorPathStartAbs)} is not null)");
        }

        return null;
    }
}
