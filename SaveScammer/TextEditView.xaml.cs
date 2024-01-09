using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
    /// Logika interakcji dla klasy TextEditView.xaml
    /// </summary>
    /// 

    public class EditViewSaveEventArgs: EventArgs
    {
        public string PreviousValue;
        public string CurrentValue;
        public bool ValueChanged => PreviousValue != CurrentValue;
        public EditViewSaveEventArgs(string previousValue, string currentValue) 
        {
            PreviousValue = previousValue;
            CurrentValue = currentValue;
        }
    }
    public partial class TextEditView : UserControl
    {
        public event EventHandler OnSave;
        public event EventHandler OnCancel;
        public bool EditMode { get; protected set; }
        private bool ShowButtons;
        public string Label
        {
            get => textBlock_label.Text;
            set
            {
                textBlock_label.Text = value;
            }
        }
        public string Display
        {
            get => textBlock_display.Text;
            set
            {
                textBlock_display.Text = value;
            }
        }

        public TextEditView()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public TextEditView(string label, string display, bool showButtons=false)
        {
            InitializeComponent();

            textBlock_label.Text = $"{label}:";
            textBlock_display.Text = display;
            textBlock_edit.Text = display;
            if(showButtons)
            {
                button_toggleEdit.Visibility = Visibility.Visible;
            }
            ShowButtons = showButtons;
        }

        public void ToggleMode(bool? editModeOn=null)
        {
            if (editModeOn == null)
            {
                EditMode = !EditMode;
            }
            else
            {
                EditMode = (bool)editModeOn;
            }
            if(EditMode)
            {
                if(ShowButtons)
                {
                    button_toggleEdit.Visibility = Visibility.Visible;
                    button_editCancel.Visibility = Visibility.Visible;
                }

                button_toggleEdit.Content = "Save";
                textBlock_edit.Visibility = Visibility.Visible;
                textBlock_display.Visibility = Visibility.Collapsed;
            }
            else
            {
                button_toggleEdit.Content = "Edit";
                if (!ShowButtons)
                {
                    button_toggleEdit.Visibility = Visibility.Collapsed;
                }
                textBlock_edit.Visibility = Visibility.Collapsed;
                textBlock_display.Visibility = Visibility.Visible;
                button_editCancel.Visibility = Visibility.Collapsed;
            }
        }

        private void button_toggleEdit_Click(object sender, RoutedEventArgs e)
        {
            if(EditMode)
            {
                OnSave?.Invoke(this, new EditViewSaveEventArgs(textBlock_display.Text, textBlock_edit.Text));
                textBlock_display.Text = textBlock_edit.Text;
                ToggleMode(false);
            }
            else
            {
                textBlock_edit.Text = textBlock_display.Text;
                ToggleMode(true);
            }
            
        }

        private void button_editCancel_Click(object sender, RoutedEventArgs e)
        {
            ToggleMode(false);
        }
    }
}
