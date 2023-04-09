using System.Collections.Generic;
using ActionRepeater.UI.Helpers;
using ActionRepeater.UI.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed class AddActionMenuItems
{
    private readonly AsyncRelayCommand<ActionType> _addActionCommand;

    public AddActionMenuItems(IDialogService dialogService)
    {
        _addActionCommand = new(dialogService.ShowEditActionDialog);
    }

    public void AddTo(IList<MenuFlyoutItemBase> items)
    {
        items.Add(new MenuFlyoutItem()
        {
            Text = "Key Action",
            Icon = new FontIcon() { Glyph = ActionGlyphs.Keyboard },
            Command = _addActionCommand,
            CommandParameter = ActionType.KeyAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Mouse Button Action",
            Icon = new FontIcon() { Glyph = ActionGlyphs.Mouse },
            Command = _addActionCommand,
            CommandParameter = ActionType.MouseButtonAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Mouse Wheel Action",
            Icon = new FontIcon() { Glyph = ActionGlyphs.Mouse },
            Command = _addActionCommand,
            CommandParameter = ActionType.MouseWheelAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Wait Action",
            Icon = new FontIcon() { Glyph = ActionGlyphs.Clock },
            Command = _addActionCommand,
            CommandParameter = ActionType.WaitAction,
        });
        items.Add(new MenuFlyoutSeparator());
        items.Add(new MenuFlyoutItem()
        {
            Text = "Text Type Action",
            Icon = new FontIcon() { Glyph = ActionGlyphs.Keyboard },
            Command = _addActionCommand,
            CommandParameter = ActionType.TextTypeAction,
        });
    }
}
