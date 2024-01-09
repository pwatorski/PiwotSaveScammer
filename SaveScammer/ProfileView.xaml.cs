using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace SaveScammer
{
    /// <summary>
    /// Logika interakcji dla klasy ProfileView.xaml
    /// </summary>
    /// 

    class ProgressReport
    {
        public long Elapsed { get; set; }
        public TimeSpan TimeLeft { get; set; }
        public int ProgressValue { get; set; }
        public bool Saved { get; set; }
    }
    public partial class ProfileView : System.Windows.Controls.UserControl
    {
        public ScamProfile? Profile { get; set; }
        public bool EditMode { get; set; }
        public bool ViewMode { get => !EditMode; }
        public MainWindow ParentWindow { get; }

        private string oryginalName;
        private string oryginalScamStorage;
        private string oryginalSaveTarget;

        private int oryginalIntervalSeconds;
        private int oryginalIntervalMinutes;
        private int oryginalIntervalHours;

        private int oryginalMaxSaves;

        BackgroundWorker savingWorker;

        bool SavingRunning;
        private StorageFile? curSelectedFile;
        BindingListCollectionView blcv;
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public ProfileView(MainWindow parentWindow, ScamProfile? profile)
        {
            InitializeComponent();
            DataContext = this;
            ParentWindow = parentWindow;
            SavingRunning = false;
            UpdateFromProfile(profile);

        }

        private BackgroundWorker GetNewWorker()
        {
            if(savingWorker != null && savingWorker.IsBusy)
                savingWorker.CancelAsync();
            savingWorker = new BackgroundWorker();
            savingWorker.WorkerReportsProgress = true;
            savingWorker.WorkerSupportsCancellation = true;
            savingWorker.DoWork += SavingWorker_DoWork;
            savingWorker.RunWorkerCompleted += SavingWorker_RunWorkerCompleted;
            savingWorker.ProgressChanged += SavingWorker_ProgressChanged;
            return savingWorker;
        }

        private void SavingWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            ProgressReport progressReport = e.UserState as ProgressReport;

            textBlock_timeLeft.Text = progressReport.TimeLeft.ToString(@"hh\:mm\:ss\.ff");
            progressBar_timeLeft.Value = progressReport.ProgressValue;
            if (progressReport.Saved)
            {
                listView_saves.Items.Refresh();
                textBlock_curSizeDisplay.Text = Misc.GetSizeString(Profile?.GetTotalSize() ?? 0);
            }

        }

        private void SavingWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private string GetNextSaveName(string format)
        {
            if(Profile ==  null) { return format; }
            var fileNames = Profile.StorageFiles.Select(x => x.SaveName).Where(x => x.StartsWith(format)).ToList();
            int maxNum = 0;
            foreach (var fileName in fileNames)
            {
                if(int.TryParse(fileName.Split(' ').Last(), out int numOut))
                {
                    if(numOut > maxNum)
                        maxNum = numOut;
                }
            }
            maxNum += 1;
            return $"{format} {maxNum}";
        }

        private void SavingWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;
            if (worker == null) { return; }
            Stopwatch stopwatch = Stopwatch.StartNew();
            Profile?.StoreSave(GetNextSaveName("Auto save"));
            while (!worker.CancellationPending && Profile != null)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    var saveNow = stopwatch.ElapsedMilliseconds / 1000 >= Profile.Interval;
                    var progress = new ProgressReport()
                    {
                        Elapsed = stopwatch.ElapsedMilliseconds,
                        TimeLeft = TimeSpan.FromSeconds(Profile.Interval - stopwatch.Elapsed.TotalSeconds),
                        ProgressValue = (int)Math.Ceiling(1000 * (stopwatch.Elapsed.TotalSeconds / Profile.Interval)),
                        Saved = saveNow

                    };
                    if (saveNow)
                    {
                        stopwatch.Restart();
                        Profile?.StoreSave(GetNextSaveName("Auto save"));
                    }
                    
                    worker.ReportProgress(0, progress);
                    System.Threading.Thread.Sleep(200);
                    
                }
                
            }
        }

        public void UpdateFromProfile(ScamProfile? profile)
        {
            Profile = profile;
            
            if (Profile == null)
            {
                grid_main.Visibility = Visibility.Collapsed;
                return;
            }
            Profile.LoadStorage();
            grid_main.Visibility = Visibility.Visible;
            oryginalName = Profile.Name;
            textBox_nameEdit.Text = Profile.Name;
            textBlock_headerName.Text = Profile.Name;
            textBlock_storageDisplay.Text = Profile.ScamStorageFull;
            oryginalScamStorage = Profile.ScamStorage;
            textBlock_targetDisplay.Text = Profile.SaveTarget == string.Empty ? "Target not set!" : Profile.SaveTarget;
            oryginalSaveTarget = Profile.SaveTarget;

            var interval = Profile.Interval;
            var intervalText = $"{interval%60:00}s";
            textBox_intervalEditSeconds.Text = $"{interval % 60}";
            interval /= 60;
            intervalText = $"{interval % 60:00}m " + intervalText;
            textBox_intervalEditMinutes.Text = $"{interval % 60}";
            interval /= 60;
            intervalText = $"{interval:00}h " + intervalText;
            textBox_intervalEditHours.Text = $"{interval % 60}";
            textBlock_intervalDisplay.Text = intervalText;

            oryginalMaxSaves = Profile.MaxSaves;
            textBox_maxSavesEdit.Text = $"{oryginalMaxSaves}";
            textBlock_maxSavesDisplay.Text = $"{oryginalMaxSaves}";

            listView_saves.ItemsSource = Profile.StorageFiles;
            UpdateVisibility();
            listView_saves.Items.Refresh();

        }

        public void UpdateProfile()
        {
            if ( Profile == null )
                return;
            Profile.Name = oryginalName;
            Profile.ScamStorage = oryginalScamStorage;
            Profile.SaveTarget = oryginalSaveTarget;
            Profile.Interval = oryginalIntervalSeconds + oryginalIntervalMinutes * 60 + oryginalIntervalHours * 3600;
            Profile.MaxSaves = oryginalMaxSaves;
        }

        public void ToggleMode(bool? editModeOn = null)
        {
            if (editModeOn == null)
            {
                EditMode = !EditMode;
            }
            else
            {
                EditMode = (bool)editModeOn;
            }
            UpdateVisibility();
            if (EditMode)
            {
                textBlock_editButton.Text = "Cancel";
                textBlock_storageDisplay.Text = oryginalScamStorage;
            }
            else
            {
                textBlock_editButton.Text = "Edit";
                textBlock_storageDisplay.Text = Profile.ScamStorageFull;
            }
        }

        protected void UpdateVisibility()
        {
            if (EditMode)
            {
                button_save.Visibility = Visibility.Visible;
                textBox_nameEdit.Visibility = Visibility.Visible;
                textBlock_headerName.Visibility = Visibility.Collapsed;
                button_storageEdit.Visibility = Visibility.Visible;
                stackPanel_targetButtons.Visibility = Visibility.Visible;
                stackPanel_intervalEdit.Visibility = Visibility.Visible;
                textBlock_intervalDisplay.Visibility = Visibility.Collapsed;
                textBox_maxSavesEdit.Visibility = Visibility.Visible;
                textBlock_maxSavesDisplay.Visibility = Visibility.Collapsed;
                button_namedSave.IsEnabled = false;
                button_snapshot.IsEnabled = false;
                button_toggleSaving.IsEnabled = false;
            }
            else
            {
                button_save.Visibility = Visibility.Collapsed;
                textBox_nameEdit.Visibility = Visibility.Collapsed;
                textBlock_headerName.Visibility = Visibility.Visible;
                button_storageEdit.Visibility = Visibility.Collapsed;
                stackPanel_targetButtons.Visibility = Visibility.Collapsed;
                stackPanel_intervalEdit.Visibility = Visibility.Collapsed;
                textBlock_intervalDisplay.Visibility = Visibility.Visible;
                textBox_maxSavesEdit.Visibility = Visibility.Collapsed;
                textBlock_maxSavesDisplay.Visibility = Visibility.Visible;
                button_namedSave.IsEnabled = true;
                button_snapshot.IsEnabled = true;
                button_toggleSaving.IsEnabled = true;
            }

            foreach(var sf in Profile.StorageFiles)
            {
                sf.ViewMode = ViewMode;
            }
        }

        private void button_edit_cancel_Click(object sender, RoutedEventArgs e)
        {
            ToggleMode();
            listView_saves.Items.Refresh();
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            oryginalName = textBox_nameEdit.Text;
            ToggleMode(false);
            listView_saves.Items.Refresh();
            UpdateProfile();
            UpdateFromProfile(Profile);
            Profile?.Save();
            ParentWindow.ProfileSaved(Profile);
        }

        private void textBox_intervalEditSeconds_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox_intervalEditSeconds.Text.Length == 0)
            {
                oryginalIntervalSeconds = 0;
            }
            else if (int.TryParse(textBox_intervalEditSeconds.Text, out int outVal))
            {
                oryginalIntervalSeconds = outVal;
            }
            else
            {
                textBox_intervalEditSeconds.Text = oryginalIntervalSeconds.ToString();
            }
        }

        private void textBox_intervalEditMinutes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox_intervalEditMinutes.Text.Length == 0)
            {
                oryginalIntervalMinutes = 0;
            }
            else if (int.TryParse(textBox_intervalEditMinutes.Text, out int outVal))
            {
                oryginalIntervalMinutes = outVal;
            }
            else
            {
                textBox_intervalEditMinutes.Text = oryginalIntervalMinutes.ToString();
            }
        }

        private void textBox_intervalEditHours_TextChanged(object sender, TextChangedEventArgs e)
        {   
            if (textBox_intervalEditHours.Text.Length == 0)
            {
                oryginalIntervalHours = 0;
            }    
            else if ( int.TryParse(textBox_intervalEditHours.Text, out int outVal))
            {
                oryginalIntervalHours = outVal;
            }
            else
            {
                textBox_intervalEditHours.Text = oryginalIntervalHours.ToString();
            }
        }

        private void button_storageEdit_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.InitialDirectory = textBlock_storageDisplay.Text;
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBlock_storageDisplay.Text = fbd.SelectedPath;
                    oryginalScamStorage = fbd.SelectedPath;
                }

            }
        }

        private void button_targetEditFile_Click(object sender, RoutedEventArgs e)
        {
            using (var ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.InitialDirectory = textBlock_targetDisplay.Text;
                System.Windows.Forms.DialogResult result = ofd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBlock_targetDisplay.Text = ofd.FileName;
                    oryginalSaveTarget = ofd.FileName;
                }

            }
        }

        private void button_targetEditDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.InitialDirectory = textBlock_targetDisplay.Text;
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBlock_targetDisplay.Text = fbd.SelectedPath;
                    oryginalSaveTarget = fbd.SelectedPath;
                }

            }
        }

        private void listView_saves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button_saveLoad_Click(object sender, RoutedEventArgs e)
        {
            Button? b = sender as Button;

            StorageFile? storageFile = b?.CommandParameter as StorageFile;
            if (storageFile == null)
                return;
            Profile?.RestoreSave(storageFile.StorageFilePath);
        }

        private void button_saveDelete_Click(object sender, RoutedEventArgs e)
        {
            Button? b = sender as Button;

            StorageFile? storageFile = b?.CommandParameter as StorageFile;
            if (storageFile == null)
                return;
            Profile?.DeleteStoredSave(storageFile);
            listView_saves.Items.Refresh();
            textBlock_curSizeDisplay.Text = Misc.GetSizeString(Profile?.GetTotalSize() ?? 0);
        }

        private void button_snapshot_Click(object sender, RoutedEventArgs e)
        {
            Profile?.StoreSave(GetNextSaveName("Snapshot"));
            listView_saves.Items.Refresh();
            textBlock_curSizeDisplay.Text = Misc.GetSizeString(Profile?.GetTotalSize()??0);
        }

        private void button_toggleSaving_Click(object sender, RoutedEventArgs e)
        {
            SavingRunning = !SavingRunning;

            if (SavingRunning)
            {
                textBlock_toggleSaving.Text = "Disable auto-saving";
                if (savingWorker == null || !savingWorker.IsBusy)
                {
                    savingWorker = GetNewWorker();
                    savingWorker.RunWorkerAsync();
                }
                else
                {

                }
                
            }
            else
            {
                if (savingWorker != null && savingWorker.IsBusy)
                {
                    savingWorker.CancelAsync();
                }
                else
                {
                    
                }

                textBlock_toggleSaving.Text = "Enable auto-saving";
                progressBar_timeLeft.Value = 0;
                textBlock_timeLeft.Text = "00:00:00.00";
            }
        }

        private void button_duplicate_Click(object sender, RoutedEventArgs e)
        {
            ScamProfile profile = Profile.Duplicate();

            ParentWindow.AddNewProfile(profile);
        }

        private void button_saveRename_Click(object sender, RoutedEventArgs e)
        {
            Button? b = sender as Button;

            StorageFile? storageFile = b?.CommandParameter as StorageFile;
            if (storageFile == null)
                return;
            curSelectedFile = storageFile;
            grid_rename.Visibility = Visibility.Visible;
            textBox_reame.Text = storageFile.SaveName;
            textBox_reame.Focus();
            textBox_reame.SelectAll();
        }

        private void button_namedSave_Click(object sender, RoutedEventArgs e)
        {
            grid_newName.Visibility = Visibility.Visible;
            textBox_newName.Text = "New save";
            textBox_newName.Focus();
            textBox_newName.SelectAll();
        }

        private void button_cancelNewName_Click(object sender, RoutedEventArgs e)
        {
            grid_newName.Visibility = Visibility.Collapsed;
        }

        private void button_acceptNewName_Click(object sender, RoutedEventArgs e)
        {
            var storedFile = Profile?.StoreSave(textBox_newName.Text);
            if (storedFile == null) return;
            listView_saves.Items.Refresh();
            textBlock_curSizeDisplay.Text = Misc.GetSizeString(Profile?.GetTotalSize() ?? 0);

            grid_newName.Visibility = Visibility.Collapsed;
        }

        private void listView_saves_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
            e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }
                    GridViewHeaderRowPresenter presenter = headerClicked.Parent as GridViewHeaderRowPresenter;
                    if (presenter != null)
                    {
                        int zeroBasedDisplayIndex = presenter.Columns.IndexOf(headerClicked.Column);
                        Profile.SortSaves(zeroBasedDisplayIndex, direction);
                    }

                    listView_saves.Items.Refresh();
                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void button_acceptReame_Click(object sender, RoutedEventArgs e)
        {
            var storedFile = curSelectedFile;
            if (storedFile == null) return;
                storedFile.Rename(textBox_reame.Text);
            listView_saves.Items.Refresh();
            grid_rename.Visibility = Visibility.Collapsed;
            
        }

        private void button_cancelReame_Click(object sender, RoutedEventArgs e)
        {
            grid_rename.Visibility = Visibility.Collapsed;
        }

        private void button_showTarget_Click(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(Profile.SaveTarget))
                Process.Start("explorer.exe", Profile.SaveTarget);
            else
                Process.Start("explorer.exe", Path.GetDirectoryName(Profile.SaveTarget));
        }

        private void button_showStorage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Profile.ScamStorageFull);
        }

        private void textBox_newName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            var storedFile = Profile?.StoreSave(textBox_newName.Text);
            if (storedFile == null) return;
            listView_saves.Items.Refresh();
            textBlock_curSizeDisplay.Text = Misc.GetSizeString(Profile?.GetTotalSize() ?? 0);

            grid_newName.Visibility = Visibility.Collapsed;
        }

        private void textBox_reame_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            var storedFile = curSelectedFile;
            if (storedFile == null) return;
            storedFile.Rename(textBox_reame.Text);
            listView_saves.Items.Refresh();
            grid_rename.Visibility = Visibility.Collapsed;
        }
    }


}
