using System;
using System.Windows.Forms;

namespace BilliardsApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(new Form1());
        }
    }
}