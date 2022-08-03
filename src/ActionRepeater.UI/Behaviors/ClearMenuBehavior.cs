using System.Collections.Generic;
using System.Collections.Specialized;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Behaviors;

public class ClearMenuBehavior
{
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


    private static RelayCommand? _clearActionsCommand;
    private static IRelayCommand ClearActionsCommand => _clearActionsCommand
        ??= new(ActionManager.ClearActions, static () => ActionManager.Actions.Count > 0);

    private static RelayCommand? _clearCursorPathCommand;
    private static IRelayCommand ClearCursorPathCommand => _clearCursorPathCommand
        ??= new(ActionManager.ClearCursorPath, static () => ActionManager.CursorPathStart is not null);

    private static RelayCommand? _clearAllCommand;
    private static IRelayCommand ClearAllCommand => _clearAllCommand
        ??= new(ActionManager.ClearAll, static () => ActionManager.Actions.Count > 0 || ActionManager.CursorPathStart is not null);

    private static bool _isSubscribedToColChanged = false;

    private static void OnMenuItemListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var items = (IList<MenuFlyoutItemBase>?)e.NewValue;
        if (items is null) return;

        // these items cant be cached
        items.Add(new MenuFlyoutItem()
        {
            Text = "Clear Actions",
            Command = ClearActionsCommand,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Clear Cursor Path",
            Command = ClearCursorPathCommand,
        });
        items.Add(new MenuFlyoutItem()
        {
            Text = "Clear All",
            Command = ClearAllCommand,
        });

        if (!_isSubscribedToColChanged)
        {
            ActionManager.ActionsCountChanged += ActionManager_ActionsCountChanged;
            ActionManager.CursorPathStartChanged += ActionManager_CursorPathStartChanged;
            _isSubscribedToColChanged = true;
        }
    }

    private static void ActionManager_ActionsCountChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ClearAllCommand.NotifyCanExecuteChanged();
        ClearActionsCommand.NotifyCanExecuteChanged();
    }

    private static void ActionManager_CursorPathStartChanged(object? sender, MouseMovement? e)
    {
        ClearAllCommand.NotifyCanExecuteChanged();
        ClearCursorPathCommand.NotifyCanExecuteChanged();
    }
}
