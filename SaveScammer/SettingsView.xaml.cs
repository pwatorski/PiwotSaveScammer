using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace SaveScammer
{
    /// <summary>
    /// Logika interakcji dla klasy SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        bool EditingInterval;
        public SettingsView()
        {
            InitializeComponent();
            textBlock_defaultPath.Text = Settings.SettingsStoragePath;
            textBlock_defaultStorage.Text = Settings.StoragePath;
        }

        private void button_defaultPath_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.InitialDirectory = textBlock_defaultPath.Text;
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBlock_defaultPath.Text = fbd.SelectedPath;
                    Settings.SettingsStoragePath = fbd.SelectedPath;
                    Settings.Save();
                }
            }
        }

        private void button_defaultStorage_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.InitialDirectory = textBlock_defaultStorage.Text;
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBlock_defaultStorage.Text = fbd.SelectedPath;
                    Settings.StoragePath = fbd.SelectedPath;
                    Settings.Save();
                }
            }
        }

        private void button_defaultPathOpen_Click(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(Settings.SettingsStoragePath))
            {
                Process.Start("explorer.exe", Settings.SettingsStoragePath);
            }
        }

        private void button_defaultStorageOpen_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Settings.StoragePath))
            {
                Process.Start("explorer.exe", Settings.StoragePath);
            }
        }

        private void button_defaultInterval_Click(object sender, RoutedEventArgs e)
        {
            EditingInterval = !EditingInterval;
            if (EditingInterval)
            {
                textBlock_defaultInterval.Visibility = Visibility.Collapsed;
                stackPanel_defaultIntervalEdit.Visibility = Visibility.Visible;
                button_defaultInterval.Content = "Save";
            }
            else
            {
                textBlock_defaultInterval.Visibility = Visibility.Visible;
                stackPanel_defaultIntervalEdit.Visibility = Visibility.Collapsed;
                button_defaultInterval.Content = "Edit";
                Settings.DefaultInterval = 0;
                Settings.Save();
            }
        }

        private void button_defaultMaxAutoSaves_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_defaultIntervalCancel_Click(object sender, RoutedEventArgs e)
        {
            textBlock_defaultInterval.Visibility = Visibility.Visible;
            stackPanel_defaultIntervalEdit.Visibility = Visibility.Collapsed;
            button_defaultInterval.Content = "Edit";
        }

        private void textBox_defaultInterval_h_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox_defaultInterval_m_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox_defaultInterval_s_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
