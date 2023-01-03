﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using CardCheckAssistant.ViewModels;
using CardCheckAssistant.Services;

namespace CardCheckAssistant.Views;

public sealed partial class Step1Page : Page
{
    public Step1Page()
    {
        InitializeComponent();
    }

    public Step1PageViewModel ViewModel => DataContext as Step1PageViewModel;

    private async void MessageBox_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Message Dialog will be opened.");

        await this.MessageDialogAsync("All we are saying:", "Give peace a chance.", "Got it");

        Debug.WriteLine("Message Dialog was closed.");
    }
}
