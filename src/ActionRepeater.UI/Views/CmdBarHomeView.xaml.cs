﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ActionRepeater.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ActionRepeater.UI.Views;

public sealed partial class CmdBarHomeView : UserControl
{
    public CmdBarHomeViewModel ViewModel { get; set; } = null!;

    public CmdBarHomeView()
    {
        InitializeComponent();
    }
}