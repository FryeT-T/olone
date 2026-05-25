using System;
using System.Windows.Forms;
using game.core;

namespace game
{
    internal static class Program
    {
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameWindow() { WindowState = FormWindowState.Maximized, FormBorderStyle = FormBorderStyle.None });
        }
    }
}