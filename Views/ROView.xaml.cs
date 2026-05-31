using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BayBrain.Views
{
    public partial class ROView : UserControl
    {
        public ROView()
        {
            InitializeComponent();
        }

        private void NumberOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => !char.IsDigit(c));
        }

        private void NumberOnlyTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            var text = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;
            if (text.Any(c => !char.IsDigit(c)))
            {
                e.CancelCommand();
            }
        }
    }
}
