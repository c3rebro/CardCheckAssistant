using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using CardCheckAssistant.Views;

using Log4CSharp;

using System.Windows.Input;

namespace CardCheckAssistant.ViewModels;

/// <summary>
/// 
/// </summary>
public class AboutPageViewModel : ObservableObject
{
    /// <summary>
    /// 
    /// </summary>
    public AboutPageViewModel()
    {
        NextStepCanExecute = false;
        GoBackCanExecute = true;
    }

    #region ObservableObjects

    /// <summary>
    /// 
    /// </summary>
    public bool NextStepCanExecute
    {
        get => _nextStepCanExecute;
        set => SetProperty(ref _nextStepCanExecute, value);
    }
    private bool _nextStepCanExecute;

    /// <summary>
    /// 
    /// </summary>
    public bool GoBackCanExecute
    {
        get => _goBackCanExecute;
        set => SetProperty(ref _goBackCanExecute, value);
    }
    private bool _goBackCanExecute;

    #endregion

    #region Commands

    /// <summary>
    /// 
    /// </summary>
    public ICommand PostPageLoadedCommand => new AsyncRelayCommand(PostPageLoadedCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public ICommand NavigateNextStepCommand => new RelayCommand(NavigateNextStepCommand_Executed);

    /// <summary>
    /// 
    /// </summary>
    public ICommand NavigateBackCommand => new RelayCommand(NavigateBackCommand_Executed);

    #endregion

    #region Extension Methods

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task PostPageLoadedCommand_Executed()
    {
        try
        {

        }
        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async void NavigateNextStepCommand_Executed()
    {
        try
        {
            var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
            var navigation = window.Navigation;
            var step2Page = navigation.GetNavigationViewItems(typeof(Step2Page)).First();
            navigation.SetCurrentNavigationViewItem(step2Page);
            step2Page.IsEnabled = true;
        }

        catch (Exception e)
        {
            LogWriter.CreateLogEntry(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void NavigateBackCommand_Executed()
    {
        var window = (Application.Current as App)?.Window as MainWindow ?? new MainWindow();
        var navigation = window.Navigation;
        var homePage = navigation.GetNavigationViewItems(typeof(HomePage)).First();
        navigation.SetCurrentNavigationViewItem(homePage);
    }
}
