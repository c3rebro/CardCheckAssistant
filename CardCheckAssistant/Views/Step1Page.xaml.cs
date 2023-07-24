using Microsoft.UI.Xaml;
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
}
