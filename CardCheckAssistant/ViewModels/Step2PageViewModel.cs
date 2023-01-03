using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CardCheckAssistant.Models;
using CardCheckAssistant.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace CardCheckAssistant.ViewModels;

public class Step2PageViewModel : ObservableObject
{
    public Step2PageViewModel()
    {
        ChipCount = new ObservableCollection<string>();

        for (var i = 0; i <= 10; i++)
        {
            ChipCount.Add(i.ToString("D2"));
        }

        NumberOfDeliveredChips = ChipCount.First();
        NextStepCanExecute = true;
        GoBackCanExecute = true;
    }

    public bool NextStepCanExecute
    {
        get => _nextStepCanExecute;
        set => SetProperty(ref _nextStepCanExecute, value);
    }
    private bool _nextStepCanExecute;

    public bool GoBackCanExecute
    {
        get => _goBackCanExecute;
        set => SetProperty(ref _goBackCanExecute, value);
    }
    private bool _goBackCanExecute;

    public string JobNumber => string.Format("JobNr.: {0}; {1}",CheckProcessService.CurrentCustomer.JobNr, CheckProcessService.CurrentCustomer.CName);

    public string NumberOfDeliveredChips
    {
        get => _numberOfDeliveredChips;
        set
        {
            if (int.Parse(value) > 0)
            {
                NextStepCanExecute = true;
            }
            else
            {
                NextStepCanExecute = false;
            }
            SetProperty(ref _numberOfDeliveredChips, value);
        }
    }
    private string _numberOfDeliveredChips;
    
    public ICommand ConfirmationCommandYesNo => new AsyncRelayCommand(ConfirmationYesNo_Executed);

    public ICommand ConfirmationCommandYesNoCancel => new AsyncRelayCommand(ConfirmationYesNoCancel_Executed);

    public ICommand InputStringCommand => new AsyncRelayCommand(InputString_Executed);
    
    public ObservableCollection<string> ChipCount { get; set; }

    
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
    
}
