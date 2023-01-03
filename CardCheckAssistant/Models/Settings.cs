using CommunityToolkit.Mvvm.ComponentModel;

namespace CardCheckAssistant.Models
{
    public partial class Settings : ObservableObject
    {
        [ObservableProperty]
        private bool isLightTheme;

        public Settings()
        {
            // Required for serialization.
        }
    }
}