﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using CardCheckAssistant.ViewModels;
using CardCheckAssistant.Services;

namespace CardCheckAssistant.Views;

public sealed partial class Step2Page : Page
{
    public Step2Page()
    {
        InitializeComponent();
    }

    public Step2PageViewModel ViewModel => DataContext as Step2PageViewModel;

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PostPageLoadedCommand.Execute(null);
    }

}
