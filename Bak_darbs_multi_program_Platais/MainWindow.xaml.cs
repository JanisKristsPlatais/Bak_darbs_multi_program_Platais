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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bak_darbs_multi_program_Platais.Database;
using static Bak_darbs_multi_program_Platais.MainWindow;
using Bak_darbs_multi_program_Platais.Models;
using Bak_darbs_multi_program_Platais.Services;
using System.Runtime.InteropServices;

namespace Bak_darbs_multi_program_Platais
{
    public partial class MainWindow : Window
    {
        private List<ProgramModel> programs = new List<ProgramModel>();
        private CustomHotkey launchHotkey = new CustomHotkey { Ctrl = true, MainKey = Key.Q }; //default is Ctrl+Q
        private bool isCapturingHotkey = false;

        private string currentProfileName = "Profile";

        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            UpdateHotkeyLabel();
            CreateEmptyProgramButton();
        }


        //Hotkey management-----------------


        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e) { //if hotkey is pressed, launches all programs
            if (launchHotkey != null && launchHotkey.isMatch(e)) { 
                LaunchAllButton_Click(null, null);
                e.Handled = true;
            }
        }
        private void ChangeHotkeyButton_Click(object sender, RoutedEventArgs e) { //lets user change hotkey
            if (!isCapturingHotkey) { 
                isCapturingHotkey = true;
                HotkeyLabel.Content = ("Press your hotkey combination. (ESC to cancel)");
                this.PreviewKeyDown -= CaptureHotkey;
                this.PreviewKeyDown += CaptureHotkey;
            }
        }
        private void CaptureHotkey(object sender, KeyEventArgs e) {
            e.Handled = true;
            if (e.Key == Key.Escape) { //if user presses ESC, end hotkey capture
                EndHotkeyCapture();
                return;
            }
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin) { return; } //skip modifier keys
            launchHotkey = new CustomHotkey
            {
                Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                Alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt),
                MainKey = key
            };

            UpdateHotkeyLabel();
            MessageBox.Show($"New hotkey set: {launchHotkey}");
            EndHotkeyCapture();
        }
        private void EndHotkeyCapture() { 
            this.PreviewKeyDown -= CaptureHotkey;
            isCapturingHotkey = false;
            HotkeyLabel.Content = $"Current: {launchHotkey}";
        }
        private void UpdateHotkeyLabel() { HotkeyLabel.Content = $"Current: {launchHotkey}"; } //updates hotkey label




        // Program tiles-buttons
        private Button CreateControlButton(string content, RoutedEventHandler clickHandler, Thickness margin, ProgramModel program = null) {
            var button = new Button //make default button
            {
                Content = content,
                Width = 40,
                Height = 20,
                Margin = margin,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = program
            };
            button.Click += clickHandler;
            return button;
        }
        private Grid CreateProgramGrid(Button programButton, ProgramModel program) { //defualt program grid with empty buttons
            var grid = new Grid();
            grid.Children.Add(programButton);

            var closeButton = CreateControlButton("X", CloseButton_Click, new Thickness(0, 10, 10, 0));
            grid.Children.Add(closeButton);

            if (program != null) { //if its a program button (has path), also add open button to it
                var openButton = CreateControlButton("O", OpenButton_Click, new Thickness(0, 10, 45, 0), program);
                grid.Children.Add(openButton);
            }
            return grid;

        }
        //empty and add button------------
        private void AddEmptyButton_Click(object sender, RoutedEventArgs e){ AddEmptyProgramTile(); }
        private void CreateEmptyProgramButton() {
            for (int i = 0; i < 12; i++) {       // how many empty buttons
                AddEmptyProgramTile();}}
        private void AddEmptyProgramTile(){ //make and add empty button to tile layout
            var emptyButton = new Button{
                Content = "Click or Drag here",
                Width = 150,
                Height = 100,
                Margin = new Thickness(10),
                Tag = null
            };
            emptyButton.Click += ProgramButton_Click;
            emptyButton.AllowDrop = true;
            emptyButton.Drop += ProgramButton_Drop;

            var grid = CreateProgramGrid(emptyButton, null);
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(grid);

            ProgramsWrapPanel.Children.Insert(ProgramsWrapPanel.Children.Count - 1, stackPanel);
            UpdateAddButtonVisibility();
        }
       

        private void UpdateAddButtonVisibility() { 
            int programTileCount = ProgramsWrapPanel.Children
                .OfType<StackPanel>().
                Count(sp=>sp.Children.OfType<Grid>().Any());        //count program buttons

            if(programTileCount >= 50) AddEmptyButton.Visibility = Visibility.Collapsed; //max 50 empty/program buttons
            else AddEmptyButton.Visibility = Visibility.Visible;
        }


        private void CreateProgramTile(ProgramModel program) {  //tile for each program
        var programButton = new Button { 
            Content = program.Name, 
            Width = 150, Height = 100, 
            Margin = new Thickness(10), 
            Tag = program }; //store program info in button
            programButton.Click += ProgramButton_Click;
            programButton.AllowDrop = true;
            programButton.Drop += ProgramButton_Drop;

            var grid = CreateProgramGrid(programButton, null);
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(grid);

            ProgramsWrapPanel.Children.Add(stackPanel);
        }


        //program button------------
        private void ProgramButton_Click(object sender, RoutedEventArgs e) { //when clicking on program button, open Info Window
            var button = sender as Button;
            var program = button?.Tag as ProgramModel;
            if (program == null){ //no info => enter name, path
                var inputWindow = new ProgramInfoWindow(program);
                inputWindow.OnSubmit += (name, path, x, y) =>
                {
                    program = new ProgramModel { Name = name, ProgramName = System.IO.Path.GetFileName(path), Path = path , X = x, Y = y};
                    button.Content = name;
                    button.Tag = program;
                    ConvertToProgramButton(button, program);
                };
                inputWindow.Show();
            } else {    //has info => update info
                var infoWindow = new ProgramInfoWindow(program);

                infoWindow.OnSubmit += (newName, newPath, newX, newY) => { 
                    program.Name = newName;
                    program.ProgramName = System.IO.Path.GetFileName(newPath);
                    program.Path = newPath;
                    program.X = newX;
                    program.Y = newY;
                    button.Content = newName;
                };
                infoWindow.Show();
            }
        }
        private void ProgramButton_Drop(object sender, DragEventArgs e){ //handles drag-and-drop of a program onto the button
            var button = sender as Button;

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { 
                var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop); //dropped might be multiple files
                if (filePaths.Length > 0) { 
                    string filePath = filePaths[0]; //only use first file from dropped ones
                    var program = new ProgramModel { Name = System.IO.Path.GetFileName(filePath), Path = filePath };
                    button.Content = program.Name;
                    button.Tag = program;
                    ConvertToProgramButton(button, program);
                }
            }
        }
        private void ConvertToProgramButton(Button button, ProgramModel program){ //make empty button into program button
            var oldGrid = button.Parent as Grid;
            oldGrid?.Children.Remove(button);

            var newGrid = CreateProgramGrid(button, program);
            var parentStack = oldGrid?.Parent as StackPanel;
            if (parentStack != null)
            {
                parentStack.Children.Clear();
                parentStack.Children.Add(newGrid);

            }
            button.Click -= ProgramButton_Click; //clear old
            button.Click += ProgramButton_Click; //add new
        }


        //Other buttons------------
        private void CloseButton_Click(object sender, RoutedEventArgs e) //close selected empty/program button
        {
            var button = sender as Button;
            var grid = button.Parent as Grid;
            if (grid != null) {
                var stackPanel = grid.Parent as StackPanel;
                if (stackPanel != null) { 
                    ProgramsWrapPanel.Children.Remove(stackPanel); 
                    UpdateAddButtonVisibility();
                }
            }
        }
        private async void OpenButton_Click(object sender, RoutedEventArgs e) //open program and move it to coords | open website in new tab
        {
            int findTimeout = 6000; //in miliseconds
            var button = sender as Button;
            var program = button.Tag as ProgramModel;
            if (program != null && !string.IsNullOrWhiteSpace(program.Path)) {
                try{
                    var process = System.Diagnostics.Process.Start(program.Path);
                    if (process != null){
                        await Task.Delay(500);
                        IntPtr windowHandle = IntPtr.Zero; //IntrPtr.Zero is like null
                        DateTime startTime = DateTime.Now;
                        while (windowHandle == IntPtr.Zero && (DateTime.Now - startTime).TotalMilliseconds < findTimeout){ //find window handle during time
                            windowHandle = WindowManager.FindWindow(null, program.ProgramName);
                            await Task.Delay(250);} //gives a bit of time for program to open
                        if (windowHandle != IntPtr.Zero) //if program window handle found
                        {
                            await Task.Delay(500);
                            WindowManager.ShowWindow(windowHandle, 9); //window in restored state to move easier
                            WindowManager.MoveWindow(program.ProgramName, program.X, program.Y);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open program: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else MessageBox.Show($"Program path not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

        }

        private void LaunchAllButton_Click(object sender, RoutedEventArgs e) //launch all programs = press all open buttons
        {
            foreach (var stackPanel in ProgramsWrapPanel.Children.OfType<StackPanel>())
            {
                if (stackPanel.Children[0] is Grid grid)
                {
                    foreach (var child in grid.Children.OfType<Button>())
                    {
                        if (child.Tag is ProgramModel program && program != null && !string.IsNullOrWhiteSpace(program.Path) && child.Content.ToString() == "O")
                        {
                            RoutedEventArgs args = new RoutedEventArgs();
                            OpenButton_Click(child, args);

                        }
                    }
                }
            }
        }



        //Profile------------
        private void ProfileCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProfile = ProfileCombobox.SelectedItem?.ToString();
            MessageBox.Show($"Selected Profile: {selectedProfile}");
        }
    }
}
