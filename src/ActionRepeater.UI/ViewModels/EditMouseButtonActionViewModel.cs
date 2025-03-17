using System;
using System.Collections.Generic;
using System.Linq;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public sealed partial class EditMouseButtonActionViewModel : ObservableObject
{
    public MouseButtonActionType Type
    {
        get => (MouseButtonActionType)SelectedTypeIndex;
        set => SelectedTypeIndex = (int)value;
    }

    public MouseButton Button
    {
        get => (MouseButton)SelectedButtonIndex;
        set => SelectedButtonIndex = (int)value;
    }

    public Win32.POINT Position
    {
        get => new(PositionX, PositionY);
        set
        {
            PositionX = value.x;
            PositionY = value.y;
        }
    }

    public IEnumerable<string> MBActionTypesFriendlyNames => Enum.GetNames<MouseButtonActionType>().Select(x => x.AddSpacesBetweenWords());

    [ObservableProperty]
    private int _selectedTypeIndex;

    [ObservableProperty]
    private int _selectedButtonIndex;

    [ObservableProperty]
    private int _positionX;

    [ObservableProperty]
    private int _positionY;

    public EditMouseButtonActionViewModel() { }

    public EditMouseButtonActionViewModel(MouseButtonAction mouseButtonAction)
    {
        Type = mouseButtonAction.ActionType;
        Button = mouseButtonAction.Button;
        Position = mouseButtonAction.Position;
    }
}
