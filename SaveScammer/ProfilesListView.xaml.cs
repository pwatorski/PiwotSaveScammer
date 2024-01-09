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
    /// Logika interakcji dla klasy ProfilesListView.xaml
    /// </summary>
    public partial class ProfilesListView : UserControl
    {
        int highlightedObjctId = 0;
        MainWindow parentWindow;
        public ProfilesListView(MainWindow parentWindow)
        {
            this.DataContext = this;
            InitializeComponent();
            this.parentWindow = parentWindow;
            if (!Directory.Exists(Settings.ProfilesStoragePath))
            {
                Directory.CreateDirectory(Settings.ProfilesStoragePath);
            }
            listView_profiles.ItemsSource = DataStore.ScamProfiles;
            listView_profiles.Items.Refresh();
            if(listView_profiles.Items.Count > 0)
            {
                listView_profiles.SelectedIndex = 0;
            }
        }

        private void listView_profiles_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void button_profileDelete_Click(object sender, RoutedEventArgs e)
        {
            Button? b = sender as Button;

            ScamProfile? profile = b?.CommandParameter as ScamProfile;
            if (profile == null)
                return;

            if(Keyboard.Modifiers != ModifierKeys.Shift)
            {
                var res = MessageBox.Show($"Are you sure you want to delete the following profile?\n\"{profile.Name}\"\nThis wil result in removing {profile.StorageFiles.Count} save file(s) ({Misc.GetSizeString(profile.GetTotalSize())})", "Deleting a profile", MessageBoxButton.YesNo); 
                if (res == MessageBoxResult.No) { return; }
            }
            profile.StopAll();
            profile.Delete();
            DataStore.ScamProfiles.Remove(profile);
            listView_profiles.Items.Refresh();
        }

        internal void UpdateList()
        {
            listView_profiles.Items.Refresh();
        }

        internal void Select(ScamProfile newProfile)
        {
            listView_profiles.SelectedItem = newProfile;
        }

        private void listView_profiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScamProfile? previousProfile = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as ScamProfile : null;
            ScamProfile? curentProfile = e.AddedItems.Count > 0 ? e.AddedItems[0] as ScamProfile : null;
            parentWindow.SelectionChanged(previousProfile, curentProfile);
        }
    }
}
