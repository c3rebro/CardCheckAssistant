using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using CardCheckAssistant.Views;

using Log4CSharp;

using System.Windows.Input;

namespace CardCheckAssistant.ViewModels;

public class AboutPageViewModel : ObservableRecipient
{
    /// <summary>
    /// 
    /// </summary>
    public AboutPageViewModel()
    {
        NextStepCanExecute = false;
        GoBackCanExecute = true;

        (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.GoBack();
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
    public IAsyncRelayCommand NavigateNextStepCommand => new AsyncRelayCommand(NavigateNextStepCommand_Executed);

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
    private async Task NavigateNextStepCommand_Executed()
    {
        try
        {
            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
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
        (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
    }
}
