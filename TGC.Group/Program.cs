﻿using System;
using System.Windows.Forms;
using TGC.Group.Form;

namespace TGC.Group
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm());
        }
        public static void Cerrar()
        {
            Application.Exit();
        }
    }
}