using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProfilesListView ProfilesListView { get; set; }
        ProfileView ProfileView { get; set; }

        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
            if(!Directory.Exists(Settings.ProfilesStoragePath))
            {
                Directory.CreateDirectory(Settings.ProfilesStoragePath);
            }

            ProfileView = new ProfileView(this, null);
            grid_profileViewContainer.Children.Add(ProfileView);
            
            ProfilesListView = new ProfilesListView(this);
            grid_profileListContainer.Children.Add(ProfilesListView);
            

        }

        private void listView_profiles_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void button_addProfile_Click(object sender, RoutedEventArgs e)
        {
            var newProfile = ScamProfile.GetDefaultProfile();
            newProfile.Save();
            DataStore.ScamProfiles.Add(newProfile);
            ProfilesListView.UpdateList();
            ProfilesListView.Select(newProfile);
        }

        internal void SelectionChanged(ScamProfile? previousProfile, ScamProfile? curentProfile)
        {
            ProfileView.UpdateFromProfile(curentProfile);
        }

        internal void ProfileSaved(ScamProfile? profile)
        {
            ProfilesListView.UpdateList();
        }

        internal void AddNewProfile(ScamProfile profile)
        {
            DataStore.ScamProfiles.Add(profile);
            ProfilesListView.UpdateList();
        }
    }
}
