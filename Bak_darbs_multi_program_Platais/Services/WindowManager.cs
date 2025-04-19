using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bak_darbs_multi_program_Platais.Services
{
    public class WindowManager
    {
        //basic window management functionality

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string classOfWindow, string titleOfWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr windowHandle, IntPtr windowZOrder, int newX, int newY, int width, int height, uint Flags);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr windowHandle, int nCmdShow);

        public static void MoveWindow(string windowTitle, int x, int y)
        {
            IntPtr windowHandle = FindWindow(null, windowTitle);
            if (windowHandle != IntPtr.Zero)
                SetWindowPos(windowHandle, IntPtr.Zero, x, y, 0, 0, 0x0004); //move window to coords | 0x0004 = no window z-order change
        }
    }
}
