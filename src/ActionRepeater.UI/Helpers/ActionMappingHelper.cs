using System;
using ActionRepeater.Core.Action;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.UI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Helpers;

public static class ActionMappingHelper
{
    public static ObservableObject ActionToEditViewModel(InputAction action) => action switch
    {
        KeyAction ka => new EditKeyActionViewModel(ka),
        MouseButtonAction mba => new EditMouseButtonActionViewModel(mba),
        MouseWheelAction mwa => new EditMouseWheelActionViewModel(mwa),
        WaitAction wa => new EditWaitActionViewModel(wa),
        _ => throw new NotImplementedException()
    };

    public static InputAction EditViewModelToAction(ObservableObject editVM) => editVM switch
    {
        EditKeyActionViewModel keyVM => new KeyAction(keyVM.Type, keyVM.Key),
        EditMouseButtonActionViewModel mbVM => new MouseButtonAction(mbVM.Type, mbVM.Button, mbVM.Position),
        EditMouseWheelActionViewModel mwVM => new MouseWheelAction(mwVM.HorizontalScrolling, mwVM.Steps, (int)(mwVM.DurationSecs * 1000)),
        EditWaitActionViewModel waitVM => new WaitAction((int)(waitVM.DurationSecs * 1000)),
        _ => throw new NotImplementedException()
    };

    public static ObservableObject ActionTypeToEditViewModel(ActionType actionType) => actionType switch
    {
        ActionType.KeyAction => new EditKeyActionViewModel(),
        ActionType.MouseButtonAction => new EditMouseButtonActionViewModel(),
        ActionType.MouseWheelAction => new EditMouseWheelActionViewModel(),
        ActionType.WaitAction => new EditWaitActionViewModel(),
        _ => throw new NotImplementedException()
    };

    public static ActionType EditViewModelToActionType(ObservableObject editVM) => editVM switch
    {
        EditKeyActionViewModel => ActionType.KeyAction,
        EditMouseButtonActionViewModel => ActionType.MouseButtonAction,
        EditMouseWheelActionViewModel => ActionType.MouseWheelAction,
        EditWaitActionViewModel => ActionType.WaitAction,
        _ => throw new NotImplementedException()
    };

    public static UserControl EditViewModelToEditView(ObservableObject editVM) => editVM switch
    {
        EditKeyActionViewModel vm => new EditKeyActionView() { ViewModel = vm },
        EditMouseButtonActionViewModel vm => new EditMouseButtonActionView() { ViewModel = vm },
        EditMouseWheelActionViewModel vm => new EditMouseWheelActionView() { ViewModel = vm },
        EditWaitActionViewModel vm => new EditWaitActionView() { ViewModel = vm },
        _ => throw new NotImplementedException()
    };
}

public enum ActionType
{
    KeyAction,
    MouseButtonAction,
    MouseWheelAction,
    WaitAction
}
