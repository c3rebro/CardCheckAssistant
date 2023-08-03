﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using CardCheckAssistant.ViewModels;
using CardCheckAssistant.Services;

namespace CardCheckAssistant.Views;

public sealed partial class Step3Page : Page
{
    public Step3Page()
    {
        InitializeComponent();
    }

    public Step3PageViewModel ViewModel => DataContext as Step3PageViewModel;

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PostPageLoadedCommand.Execute(null);
    }

}