using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Elatec.NET;

using Microsoft.UI.Xaml;

using CardCheckAssistant.Services;
using CardCheckAssistant.Views;

using System.Diagnostics;
using System.Windows.Input;
using System.Reflection;

namespace CardCheckAssistant.ViewModels;

public class AboutPageViewModel : ObservableRecipient
{
    private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

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
    public IAsyncRelayCommand NavigateBackCommand => new AsyncRelayCommand(NavigateBackCommand_Executed);

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
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
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
            await TWN4ReaderDevice.Instance[0].DisconnectAsync();

            (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
        }

        catch (Exception e)
        {
            eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task NavigateBackCommand_Executed()
    {
        await TWN4ReaderDevice.Instance[0].DisconnectAsync();

        (App.MainRoot.XamlRoot.Content as ShellPage)?.ViewModel.NavigationService.NavigateTo(typeof(HomePageViewModel).FullName ?? "");
    }
}
