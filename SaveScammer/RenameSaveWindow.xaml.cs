using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SaveScammer
{
    /// <summary>
    /// Logika interakcji dla klasy RenameSaveWindow.xaml
    /// </summary>
    public partial class RenameSaveWindow : Window
    {
        public string NewName { get; set; } = string.Empty;
        public bool DoRename { get; set; }
        public RenameSaveWindow()
        {
            InitializeComponent();
        }

        private void button_rename_Click(object sender, RoutedEventArgs e)
        {
            DoRename = true;
            NewName = textBox_newName.Text;
            this.Close();
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DoRename = false;
            this.Close();
        }
    }
}
