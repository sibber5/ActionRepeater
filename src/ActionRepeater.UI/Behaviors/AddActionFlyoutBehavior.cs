using System;
using System.Collections.Generic;
using ActionRepeater.UI.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ActionType = ActionRepeater.UI.ViewModels.ActionType;

namespace ActionRepeater.UI.Behaviors;

public sealed class AddActionMenuBehavior
{
    private static ContentDialogService? _contentDialogService;
    private static ContentDialogService ContentDialogService => _contentDialogService ??= App.Current.Services.GetRequiredService<ContentDialogService>();

    #region MenuItemList DP

    public static IList<MenuFlyoutItemBase>? GetMenuItemList(DependencyObject obj)
    {
        return (IList<MenuFlyoutItemBase>?)obj.GetValue(MenuItemListProperty);
    }

    public static void SetMenuItemList(DependencyObject obj, IList<MenuFlyoutItemBase>? value)
    {
        obj.SetValue(MenuItemListProperty, value);
    }

    // Using a DependencyProperty as the backing store for MenuItemList.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MenuItemListProperty =
        DependencyProperty.RegisterAttached("MenuItemList", typeof(IList<MenuFlyoutItemBase>), typeof(AddActionMenuBehavior),
            new PropertyMetadata(null, OnMenuItemListChanged));

    #endregion

    private static AsyncRelayCommand<ActionType>? _addActionCommand;
    private static IAsyncRelayCommand<ActionType> AddActionCommand => _addActionCommand
        ??= new(async (actionType) => await ContentDialogService.ShowEditActionDialog(actionType));

    private static void OnMenuItemListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var items = (IList<MenuFlyoutItemBase>?)e.NewValue;
        if (items is null) return;

        items.Add(new MenuFlyoutItem()
        {
            Text = "Key Action",
            Icon = new FontIcon() { Glyph = "\uE92E" },
            Command = AddActionCommand,
            CommandParameter = ActionType.KeyAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Mouse Button Action",
            Icon = new FontIcon() { Glyph = "\uE962" },
            Command = AddActionCommand,
            CommandParameter = ActionType.MouseButtonAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Mouse Wheel Action",
            Icon = new FontIcon() { Glyph = "\uE962" },
            Command = AddActionCommand,
            CommandParameter = ActionType.MouseWheelAction,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Wait Action",
            Icon = new FontIcon() { Glyph = "\uED5A" },
            Command = AddActionCommand,
            CommandParameter = ActionType.WaitAction,
        });
    }
}
