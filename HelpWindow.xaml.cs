﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace queriesRAMconsumption
{
    /// <summary>
    /// Логика взаимодействия для HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();

            vers.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void openLink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)); }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("For some reasons openning this link has failed.{0}Details: {1}",
                        Environment.NewLine, ex.Message),
                    "Couldn't open the link",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }

            e.Handled = true;
        }

        private void copyEmail(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(((Run)email.Inlines.FirstOrDefault()).Text);
            MessageBox.Show(
                "E-mail has been copied to the clipboard.",
                "E-mail",
                MessageBoxButton.OK,
                MessageBoxImage.Information
                );
        }
    }
}
