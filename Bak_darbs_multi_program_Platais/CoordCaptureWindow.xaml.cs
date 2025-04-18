using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Bak_darbs_multi_program_Platais
{
    /// <summary>
    /// Interaction logic for CoordCaptureWindow.xaml
    /// </summary>
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
        private void WindowMaxSize() {
            var allScreens = System.Windows.Forms.Screen.AllScreens;
            int maxRight = int.MinValue;
            int maxBottom = int.MinValue;
            int minLeft = int.MaxValue;
            int minTop = int.MaxValue;

            foreach (var screen in allScreens)
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


        private void Window_MouseMove(object sender, MouseEventArgs e){

            var screenPos = PointToScreen(e.GetPosition(this));
            CoordText.Text = $"X: {(int)screenPos.X}, Y: {(int)screenPos.Y}";
            CoordText.Visibility = Visibility.Visible;

            var pos = e.GetPosition(this); //update position of coord text
            Canvas.SetLeft(CoordText, pos.X + 15);
            Canvas.SetTop(CoordText, pos.Y + 15);
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = PointToScreen(e.GetPosition(this));
            OnPointSelected?.Invoke(pos);
            Close();
        }
    
}
}
