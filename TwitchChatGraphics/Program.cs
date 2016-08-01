// Copyright (c) 2016, Henrik Barestrand, All rights reserved.
using System;
using System.IO;
using System.Windows.Forms;

namespace ChatGraphics
{
    class Program
    {
        public static string getPath(string file)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
        }

        [STAThread]
        public static void Main()
        {
            // TestForm will containt the per-pixel-alpha stuff
            ppaForm form = new ppaForm();
            form.FormClosing += form_Closing;
            Application.Run(form);
        }

        private static void form_Closing(object sender, EventArgs e)
        {
            Environment.Exit(0); // finish form
        }
    }
}
