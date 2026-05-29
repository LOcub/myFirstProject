using System;
using System.Windows.Forms;

namespace BookManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DbHelper.InitializeDatabase();
            Application.Run(new Forms.LoginForm());
        }
    }
}
