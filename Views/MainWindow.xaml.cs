using System.Windows;

namespace BayBrain.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetService(typeof(ViewModels.MainViewModel));
        }

        private void LoginPinBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.LoginPin = LoginPinBox.Password;
            }
        }
    }
}
