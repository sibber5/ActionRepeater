﻿using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using ActionRepeater.UI.Helpers;

namespace ActionRepeater.UI.Views;

public sealed partial class EditMouseButtonActionView : UserControl
{
    public EditMouseButtonActionViewModel ViewModel { get; set; } = null!;

    public EditMouseButtonActionView()
    {
        InitializeComponent();
        
        SetNumberBoxesFormatters();
    }

    private void SetNumberBoxesFormatters()
    {
        _posXNumBox.NumberFormatter = NumberFormatterHelper.RoundToOneFormatter;
        _posYNumBox.NumberFormatter = NumberFormatterHelper.RoundToOneFormatter;
    }
}