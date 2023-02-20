using System;
using System.Collections.Generic;
using ActionRepeater.UI.Services;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Views;

public sealed class AddActionMenuItems
{
    private readonly AsyncRelayCommand<ActionType> _addActionCommand;

    public AddActionMenuItems(ContentDialogService contentDialogService)
    {
        _addActionCommand = new(async (actionType) => await contentDialogService.ShowEditActionDialog(actionType));
    }

    public void AddTo(IList<MenuFlyoutItemBase> items)
    {
        items.Add(new MenuFlyoutItem()
        {
            Text = "Key Action",
            Icon = new FontIcon() { Glyph = "\uE92E" },
            Command = _addActionCommand,
            CommandParameter = ActionType.KeyAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Mouse Button Action",
            Icon = new FontIcon() { Glyph = "\uE962" },
            Command = _addActionCommand,
            CommandParameter = ActionType.MouseButtonAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Mouse Wheel Action",
            Icon = new FontIcon() { Glyph = "\uE962" },
            Command = _addActionCommand,
            CommandParameter = ActionType.MouseWheelAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Wait Action",
            Icon = new FontIcon() { Glyph = "\uED5A" },
            Command = _addActionCommand,
            CommandParameter = ActionType.WaitAction,
        });
    }
}
