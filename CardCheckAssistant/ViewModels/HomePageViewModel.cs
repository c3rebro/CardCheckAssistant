using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using System.Collections.Generic;
using CardCheckAssistant.Views;

namespace CardCheckAssistant.ViewModels;

public class HomePageViewModel : ObservableObject
{
    public HomePageViewModel()
    {
        DataGridItemCollection = new List<Customer>()
        {
            new()
            {
                OmniLink = "https:////service.simons-voss.com//omni",
                MailTo = "mailto:john.doe@example.com",
                JobNr = "1234",
                VCNr = "00112233",
                Date = "01.01.2020",
                CName = "Robert Bosch GmbH",
                IsMember = true,
                Status = OrderStatus.Processing
            },
            new()
            {
                OmniLink = "https:////service.simons-voss.com//omni",
                MailTo = "mailto:chloe.clarkson@example.com",
                JobNr = "5678",
                Date = "02.01.2020",
                CName = "Roche Diagnostics GmbH",
                VCNr = "99882277",
                IsMember = true,
                Status = OrderStatus.Processing
            }
        };
        StartCheckCanExecute = false;

        
        var navigation = (Application.Current as App).Navigation;
        foreach (NavigationViewItem nVI in navigation.GetNavigationViewItems().Where(x => x.Content.ToString() != "Start").Where(x => x.Content.ToString().Contains("Schritt")))
        {
            nVI.IsEnabled = false;
        }
        
    }

    #region Properties

    public bool StartCheckCanExecute
    {
        get => _startCheckCanExecute;
        set => SetProperty(ref _startCheckCanExecute, value);
    }
    private bool _startCheckCanExecute;

    public Customer SelectedCustomer    
    {  
        get => selectedCustomer; 
        set { 
            if(value == null)
            {
                StartCheckCanExecute = false;
            }
            else
            {
                StartCheckCanExecute = true;
                CheckProcessService.CurrentCustomer = value;
            }
            selectedCustomer = value; } 
    }
    private Customer selectedCustomer;

    #endregion

    #region ObservableObjects

    public IEnumerable<Customer> DataGridItemCollection
    {
        get => _dataGridItemCollection;
        set => SetProperty(ref _dataGridItemCollection, value);
    }
    private IEnumerable<Customer> _dataGridItemCollection = new Customer[] { };

    #endregion

    #region Commands
    
    public ICommand ConfirmationCommandYesNo => new AsyncRelayCommand(ConfirmationYesNo_Executed);

    public ICommand ConfirmationCommandYesNoCancel => new AsyncRelayCommand(ConfirmationYesNoCancel_Executed);

    public ICommand InputStringCommand => new AsyncRelayCommand(InputString_Executed);
    
    public ICommand NavigateCommand => new RelayCommand(BeginCardCheck_Executed);

    #endregion

    
    public async void InputText_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Opening Text Input Dialog.");
        var inputText = await App.MainRoot.InputTextDialogAsync(
                "What would Faramir say?",
                "“War must be, while we defend our lives against a destroyer who would devour all; but I do not love the bright sword for its sharpness, nor the arrow for its swiftness, nor the warrior for his glory. I love only that which they defend.”\n\nJ.R.R. Tolkien"
            );

            Debug.WriteLine($"Text Input Dialog was closed with {inputText}.");
    }

    private async Task ConfirmationYesNo_Executed()
    {
        Debug.WriteLine("2-State Confirmation Dialog will be opened.");
        var confirmed = await App.MainRoot.ConfirmationDialogAsync(
                "What Pantone color do you prefer?",
                "Freedom Blue",
                "Energizing Yellow"
            );
        Debug.WriteLine($"2-State Confirmation Dialog was closed with {confirmed}.");
    }

    private async Task ConfirmationYesNoCancel_Executed()
    {
        Debug.WriteLine("3-State Confirmation Dialog will be opened.");
        var confirmed = await App.MainRoot.ConfirmationDialogAsync(
                "Is it wise to use artillery against a nuclear power plant?",
                "да",
                "That's insane",
                "I don't understand"
            );
        Debug.WriteLine($"3-State Confirmation Dialog was closed with {confirmed}.");
    }

    private async Task InputString_Executed()
    {
        Debug.WriteLine("Opening String Input Dialog.");
        var inputString = await App.MainRoot.InputStringDialogAsync(
                "How can we help you?",
                "I need ammunition, not a ride.",
                "OK",
                "Forget it"
            );
        Debug.WriteLine($"String Input Dialog was closed with '{inputString}'.");
    }
    
    public void BeginCardCheck_Executed()
    {
        var navigation = (Application.Current as App).Navigation;
        var step1Page = navigation.GetNavigationViewItems(typeof(Step1Page)).First();
        navigation.SetCurrentNavigationViewItem(step1Page);
        step1Page.IsEnabled = true;
    }
}
