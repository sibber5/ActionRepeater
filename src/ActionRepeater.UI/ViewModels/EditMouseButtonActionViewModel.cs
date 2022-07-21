using System;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActionRepeater.UI.ViewModels;

public partial class EditMouseButtonActionViewModel : ObservableObject
{
    public MouseButtonActionType Type
    {
        get => SelectedTypeIndex switch
        {
            0 => MouseButtonActionType.MouseButtonClick,
            1 => MouseButtonActionType.MouseButtonDown,
            2 => MouseButtonActionType.MouseButtonUp,
            _ => throw new NotImplementedException()
        };

        set => SelectedTypeIndex = value switch
        {
            MouseButtonActionType.MouseButtonClick => 0,
            MouseButtonActionType.MouseButtonDown => 1,
            MouseButtonActionType.MouseButtonUp => 2,
            _ => throw new NotImplementedException()
        };
    }

    public InputSimulator.MouseButton Button
    {
        get => (InputSimulator.MouseButton)SelectedButtonIndex;
        set => SelectedButtonIndex = (int)value;
    }

    public Win32.POINT Position => new(PositionX, PositionY);

    [ObservableProperty]
    private int _selectedTypeIndex;

    [ObservableProperty]
    private int _selectedButtonIndex;

    [ObservableProperty]
    private int _positionX;

    [ObservableProperty]
    private int _positionY;
}
