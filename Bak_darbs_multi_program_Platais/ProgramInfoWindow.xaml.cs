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

namespace Bak_darbs_multi_program_Platais
{
    /// <summary>
    /// Interaction logic for ProgramInfoWindow.xaml
    /// </summary>
    public partial class ProgramInfoWindow : Window
    {
        public event Action<string, string> OnSubmit;
        public ProgramInfoWindow()
        {
            InitializeComponent();
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e) { 
            var name = ProgramNameTextBox.Text;
            var path = ProgramPathTextBox.Text;
            OnSubmit?.Invoke(name, path);
            this.Close();
        }
    }
}
