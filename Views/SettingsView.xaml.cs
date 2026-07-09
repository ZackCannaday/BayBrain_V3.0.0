using System.Windows.Controls;

namespace BayBrain.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void NewUserPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel vm)
            {
                vm.NewUserPin = NewUserPinBox.Password;
            }
        }
    }
}
