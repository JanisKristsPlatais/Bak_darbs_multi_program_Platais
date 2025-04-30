using System;
using System.Windows;
using Bak_darbs_multi_program_Platais.Models;


namespace Bak_darbs_multi_program_Platais
{
    public partial class ProgramInfoWindow : Window
    {
        public event Action<string, string, int, int> OnSubmit;
        private ProgramModel program;
        public ProgramInfoWindow(ProgramModel existingProgram)
        {
            InitializeComponent();

            program = existingProgram ?? new ProgramModel(); //checks if its existing or new program
            SetProgramInfo(program);
            UpdateCoordButtonState();
        }

        //program info
        public void SetProgramInfo(ProgramModel program) {
            ProgramNameTextBox.Text = program.Name;
            ProgramPathTextBox.Text = program.Path;
            CoordsDisplayTextBox.Text = $"Coords: X = {program.X}, Y = {program.Y}";
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e){
            OnSubmit?.Invoke(ProgramNameTextBox.Text, ProgramPathTextBox.Text.Trim('"'), program.X, program.Y);
            UpdateCoordButtonState();
            this.Close();  
        }

        //program coords
        private void SetCoordsButton_Click(object sender, RoutedEventArgs e){
            if (!IsWebsite(program.Path)){
                var captureWindow = new CoordCaptureWindow();
                captureWindow.OnPointSelected = (point) =>
                {
                    program.X = (int)point.X;
                    program.Y = (int)point.Y;
                    CoordsDisplayTextBox.Text = $"Coords: X = {program.X}, Y = {program.Y}";
                };
                captureWindow.ShowDialog();
            }else MessageBox.Show("Website links open as new tabs.");
        }
        private void UpdateCoordButtonState() {
            if (IsWebsite(program.Path)){
                SetCoordsButton.Visibility = Visibility.Collapsed;
                CoordsDisplayTextBox.Text = "Website links open as new tabs";
            }
            else {
                SetCoordsButton.Visibility = Visibility.Visible;
                CoordsDisplayTextBox.Text = $"Coords: X = {program.X}, Y = {program.Y}";
            }
        }
        private bool IsWebsite(string path){
            if (path != null) {
                string lowerPath = path.ToLower();
                if (lowerPath.StartsWith("http://") || lowerPath.StartsWith("https://") || lowerPath.StartsWith("www.")) return true;
            }
            return false;
        }
    }
}
