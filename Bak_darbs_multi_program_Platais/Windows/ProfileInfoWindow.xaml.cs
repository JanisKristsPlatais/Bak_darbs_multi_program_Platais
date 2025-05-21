using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bak_darbs_multi_program_Platais.Windows
{
    public partial class ProfileInfoWindow : Window
    {
        public string ResponseText {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }
        public ProfileInfoWindow(string title, string message, string defaultValue = "")
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
            ResponseText = defaultValue;
            ResponseTextBox.SelectAll();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResponseTextBox.Text.Length > 50) {
                MessageBox.Show("Profile name cannot be over 50 characters!", "Invalid Input", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
