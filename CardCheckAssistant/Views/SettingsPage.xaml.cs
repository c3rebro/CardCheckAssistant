﻿using CardCheckAssistant.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CardCheckAssistant.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsPageViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsPageViewModel>();
        InitializeComponent();
    }
}
