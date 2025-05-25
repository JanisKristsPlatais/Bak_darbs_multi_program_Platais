using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Bak_darbs_multi_program_Platais
{
    public partial class CoordCaptureWindow : Window
    {
        public Action<Point> OnPointSelected;
        public CoordCaptureWindow()
        {
            InitializeComponent();

            WindowMaxSize();
            this.Background = new SolidColorBrush(Color.FromArgb(50,0,0,0));
            Cursor = Cursors.Cross;
        }
        private void WindowMaxSize() { //makes sure window covers all available screen space
            var allScreens = System.Windows.Forms.Screen.AllScreens; //get all screens
            //get screen edges
            int maxRight = int.MinValue;
            int maxBottom = int.MinValue;
            int minLeft = int.MaxValue;
            int minTop = int.MaxValue;

            foreach (var screen in allScreens) //makes sure all screen bounds are encompassed
            {
                minLeft = Math.Min(minLeft, screen.Bounds.Left);
                minTop = Math.Min(minTop, screen.Bounds.Top);
                maxRight = Math.Max(maxRight, screen.Bounds.Right);
                maxBottom = Math.Max(maxBottom, screen.Bounds.Bottom);
            }
            this.Width = maxRight - minLeft;
            this.Height = maxBottom - minTop;
            this.Left = minLeft;
            this.Top = minTop;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e){ //gets coords from mouse location

            var screenPos = PointToScreen(e.GetPosition(this)); //mouse position on screen
            CoordText.Text = $"X: {(int)screenPos.X}, Y: {(int)screenPos.Y}";
            CoordText.Visibility = Visibility.Visible;

            //update position of coord text, +15 to offset text 
            var pos = e.GetPosition(this); // mouse position relative to the window
            Canvas.SetLeft(CoordText, pos.X + 15);
            Canvas.SetTop(CoordText, pos.Y + 15);
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //use selected coords
        {
            var pos = PointToScreen(e.GetPosition(this));
            OnPointSelected?.Invoke(pos);
            Close();
        }
    
}
}
