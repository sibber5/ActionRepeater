using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Win32.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class EditKeyActionViewModel : ObservableValidator
{
    public KeyActionType Type
    {
        get => SelectedTypeIndex switch
        {
            0 => KeyActionType.KeyPress,
            1 => KeyActionType.KeyDown,
            2 => KeyActionType.KeyUp,
            _ => throw new NotImplementedException()
        };

        set => SelectedTypeIndex = value switch
        {
            KeyActionType.KeyPress => 0,
            KeyActionType.KeyDown => 1,
            KeyActionType.KeyUp => 2,
            _ => throw new NotImplementedException()
        };
    }

    public VirtualKey Key { get; private set; }

    public string? KeyNameErrorMessage => GetErrors(nameof(KeyName)).FirstOrDefault()?.ErrorMessage;

    public IEnumerable<string> KeyActionTypesFriendlyNames => Enum.GetNames<KeyActionType>().Select(x => x.AddSpacesBetweenWords());

    [ObservableProperty]
    private int _selectedTypeIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KeyNameErrorMessage))]
    [NotifyDataErrorInfo]
    [Required]
    [CustomValidation(typeof(EditKeyActionViewModel), nameof(ValidateKeyName))]
    private string? _keyName;

    public EditKeyActionViewModel() { }

    public EditKeyActionViewModel(KeyAction keyAction)
    {
        Type = keyAction.ActionType;
        if (ActionDescriptionTemplates.VirtualKeyFriendlyNames.TryGetValue(keyAction.Key, out string? name))
        {
            _keyName = ExcludeDescriptionFromKeyFriendlyName(name).ToString();
        }
        else
        {
            _keyName = ActionDescriptionTemplates.VirtualKeyFriendlyNames[VirtualKey.NO_KEY];
        }
        Key = keyAction.Key;
    }

    public static ValidationResult? ValidateKeyName(string? keyName, ValidationContext context)
    {
        var vm = (EditKeyActionViewModel)context.ObjectInstance;

        if (string.IsNullOrEmpty(keyName))
        {
            vm.Key = VirtualKey.NO_KEY;
            return new("Key can't be empty.");
        }

        VirtualKey keyFromTemplates = ActionDescriptionTemplates.VirtualKeyFriendlyNames.FirstOrDefault(x =>
        {
            var n = ExcludeDescriptionFromKeyFriendlyName(x.Value);
            return n.Equals(keyName.AsSpan().Trim(), StringComparison.CurrentCultureIgnoreCase);
        }).Key;

        if (keyFromTemplates != VirtualKey.NO_KEY)
        {
            vm.Key = keyFromTemplates;
            return ValidationResult.Success;
        }

        var nameSpan = keyName.Replace("WINDOWS", "WIN", StringComparison.CurrentCultureIgnoreCase).AsSpan().Trim();

        if (nameSpan.Equals("ENTER", StringComparison.CurrentCultureIgnoreCase))
        {
            vm.Key = VirtualKey.RETURN;
            return ValidationResult.Success;
        }
        else if (nameSpan.Equals("ALT", StringComparison.CurrentCultureIgnoreCase))
        {
            vm.Key = VirtualKey.MENU;
            return ValidationResult.Success;
        }
        else if (nameSpan.Equals("WIN", StringComparison.CurrentCultureIgnoreCase))
        {
            vm.Key = VirtualKey.LWIN;
            return ValidationResult.Success;
        }

        const string left = "LEFT";
        const string right = "RIGHT";

        string nameToSearch;

        if (nameSpan.StartsWith(left, StringComparison.CurrentCultureIgnoreCase))
        {
            nameToSearch = "L" + nameSpan.Slice(left.Length).TrimStart().ToString();
        }
        else if (nameSpan.StartsWith(right, StringComparison.CurrentCultureIgnoreCase))
        {
            nameToSearch = "R" + nameSpan.Slice(right.Length).TrimStart().ToString();
        }
        else if (char.IsDigit(nameSpan[0]))
        {
            nameToSearch = "KEY_" + nameSpan.ToString();
        }
        else
        {
            bool startsWithNumpad = nameSpan.StartsWith("NUMPAD", StringComparison.CurrentCultureIgnoreCase);
            nameToSearch = startsWithNumpad ? nameSpan.ToString().Replace(" ", string.Empty) : nameSpan.ToString().Replace(' ', '_');
        }

        if (Enum.TryParse<VirtualKey>(nameToSearch, true, out var key))
        {
            vm.Key = key;
            return ValidationResult.Success;
        }

        vm.Key = VirtualKey.NO_KEY;
        return new("Key doesn't exist.");
    }

    /// <summary>
    /// Removes the part in brackets, if its an unecessary description.
    /// </summary>
    private static ReadOnlySpan<char> ExcludeDescriptionFromKeyFriendlyName(ReadOnlySpan<char> name)
    {
        if (!name.EndsWith("(numpad)", StringComparison.Ordinal))
        {
            int idx = name.IndexOf('(');
            idx = idx == -1 ? name.Length : idx - 1;
            return name[..idx];
        }

        return name;
    }
}
