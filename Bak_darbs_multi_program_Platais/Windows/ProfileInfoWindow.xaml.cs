using System;
using System.Windows;

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

            Services.ThemeManager.ApplyTheme(this);
            Services.ThemeManager.ThemeChanged += OnThemeChanged;
            this.Closed += (s, e) => Services.ThemeManager.ThemeChanged -= OnThemeChanged;
        }
        private void OnThemeChanged(string newTheme) { Services.ThemeManager.ApplyTheme(this); }

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
