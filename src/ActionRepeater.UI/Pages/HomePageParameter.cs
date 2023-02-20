using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using ActionRepeater.UI.Views;

namespace ActionRepeater.UI.Pages;

public record HomePageParameter(HomePageViewModel VM, AddActionMenuItems AddActionMenuItems, Recorder Recorder, ActionListViewModel ActionListVM);
