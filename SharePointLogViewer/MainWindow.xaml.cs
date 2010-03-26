using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SharePointLogViewer.Properties;
using SharePointLogViewer.Controls.AutoCompleteTextBox;
using System.Threading;

namespace SharePointLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OverflowCollection<LogEntryViewModel> logEntries = new OverflowCollection<LogEntryViewModel>(le=>!le.Bookmarked);        

        LogsLoader logsLoader = new LogsLoader();
        LogMonitor watcher = null;
        DynamicFilter filter;
        bool liveMode;
        OpenFileDialog openDialog;
        SaveFileDialog saveDialog;
        string[] files = new string[0];
        BookmarkNavigator bookmarkNavigator;

        public static RoutedUICommand Settings = new RoutedUICommand("Settings", "Settings", typeof(MainWindow));
        public static RoutedUICommand About = new RoutedUICommand("About", "About", typeof(MainWindow));
        public static RoutedUICommand Filter = new RoutedUICommand("Filter", "Filter", typeof(MainWindow));
        public static RoutedUICommand Refresh = new RoutedUICommand("Refresh", "Refresh", typeof(MainWindow));
        public static RoutedUICommand OpenFile = new RoutedUICommand("OpenFile", "OpenFile", typeof(MainWindow));
        public static RoutedUICommand ToggleLiveMonitoring = new RoutedUICommand("ToggleLiveMonitoring", "Live", typeof(MainWindow));
        public static RoutedUICommand ExportLogEntries = new RoutedUICommand("ExportLogEntries", "ExportLogEntries", typeof(MainWindow));
        public static RoutedUICommand Previous = new RoutedUICommand("Previous", "Previous", typeof(MainWindow));
        public static RoutedUICommand Next = new RoutedUICommand("Next", "Next", typeof(MainWindow));
        public static RoutedUICommand ToggleBookmark = new RoutedUICommand("ToggleBookmark", "ToggleBookmark", typeof(MainWindow));
        public static RoutedUICommand ClearLogs = new RoutedUICommand("ClearLogs", "ClearLogs", typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
            
            if (Properties.Settings.Default.Maximized)
                WindowState = WindowState.Maximized;
            
            logsLoader.LoadCompleted += new EventHandler<LoadCompletedEventArgs>(logsLoader_LoadCompleted);
            
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

            bookmarkNavigator = new BookmarkNavigator(lstLog, ()=>GetCollectionViewSource().View);
            
            openDialog = new OpenFileDialog();
            openDialog.Multiselect = true;

            saveDialog = new SaveFileDialog();
            saveDialog.Filter = openDialog.Filter = "Log Files (*.log)|*.log";
            saveDialog.DefaultExt = ".log";

            hdrBookmark.Visible = true; // if not set the header style is not applied
            hdrTimestamp.Visible = Properties.Settings.Default.Columns.Contains("Timestamp");
            hdrProcess.Visible = Properties.Settings.Default.Columns.Contains("Process");
            hdrTid.Visible = Properties.Settings.Default.Columns.Contains("TID");
            hdrArea.Visible = Properties.Settings.Default.Columns.Contains("Area");
            hdrCategory.Visible = Properties.Settings.Default.Columns.Contains("Category");
            hdrEventID.Visible = Properties.Settings.Default.Columns.Contains("EventID");
            hdrLevel.Visible = Properties.Settings.Default.Columns.Contains("Level");
            hdrMessage.Visible = Properties.Settings.Default.Columns.Contains("Message");
            hdrCorrelation.Visible = Properties.Settings.Default.Columns.Contains("Correlation");

            lblVersion.Text = typeof(MainWindow).Assembly.GetName().Version.ToString(3);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = logEntries;
            txtFilter.AutoCompleteManager.DataProvider = new SimpleStaticDataProvider((new LogEntryTokenizer(logEntries)).Distinct());
            UpdateFilter();
            if (SPUtility.IsWSSInstalled)
            {
                openDialog.InitialDirectory = SPUtility.LogsLocations;
                openDialog.FileName = SPUtility.LatestLogFile;
            }
        }

        void logsLoader_LoadCompleted(object sender, LoadCompletedEventArgs e)
        {
            var newEntries = (from le in e.LogEntries.Skip(logEntries.Count)
                              select new LogEntryViewModel(le));
            logEntries.AddRange(newEntries);
            UpdateFilter();
            StopProcessing();
        }

        void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            if (openDialog.ShowDialog().Value)
            {
                files = openDialog.FileNames;
                openDialog.FileName = null;
                logEntries.Clear();
                LoadFiles();
                if (lstLog.Items.Count > 0)
                    lstLog.ScrollIntoView(lstLog.Items[0]);
            }
        }

        void Filter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            string criteria = cmbFilterBy.Text == "Any field" ? "*" : cmbFilterBy.Text;
            filter = DynamicFilter.Create<LogEntryViewModel>(criteria, txtFilter.Text);
            CollectionViewSource source = GetCollectionViewSource();
            if(source.View != null)
                source.View.Refresh();
        }

        CollectionViewSource GetCollectionViewSource()
        {
            CollectionViewSource source = (CollectionViewSource)this.Resources["FilteredCollection"];
            return source;
        }

        void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bdrAbout.Visibility = bdrAbout.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
                e.Handled = true;
                LoadFiles();
            }
        }

        void Refresh_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !liveMode && files.Length > 0;
        }

        void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadFiles();
        }

        void LiveMode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!SPUtility.IsWSSInstalled)
            {
                MessageBox.Show("Microsoft Sharepoint not installed on this machine");
                return;
            }
            if (liveMode)
            {
                StopLiveMonitoring();
                btnToggleLive.ToolTip = "Start Live Monitoring";
                btnToggleLive.Tag = "Images/play.png";
            }
            else
            {
                StartLiveMonitoring();
                btnToggleLive.ToolTip = "Stop Live Monitoring";
                btnToggleLive.Tag = "Images/stop.png";
            }
        }       

        void watcher_LogEntryDiscovered(object sender, LogEntryDiscoveredEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                {
                    logEntries.Add(new LogEntryViewModel(e.LogEntry));
                    lstLog.ScrollIntoView(e.LogEntry);
                }
            ));
        }

        void OfflineMode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopLiveMonitoring();
        }

        void StartLiveMonitoring()
        {
            string folderPath = SPUtility.LogsLocations;
            if (Directory.Exists(folderPath))
            {
                watcher = new LogMonitor(folderPath);
                watcher.LogEntryDiscovered += new EventHandler<LogEntryDiscoveredEventArgs>(watcher_LogEntryDiscovered);

                if (files.Length > 0)
                    Reset();

                logEntries.MaxItems = Properties.Settings.Default.LiveLimit;
                watcher.Start();
                liveMode = true;
            }
        }

        void Reset()
        {
            logEntries.Clear();
            files = new string[0];
        }

        void StopLiveMonitoring()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }

            liveMode = false;
            logEntries.MaxItems = -1;
        }

        void LoadFiles()
        {
            logsLoader.Start(files);
            StartProcessing("Loading...");
        }

        void StartProcessing(string message)
        {
            loadingAnimation.Message = message;
            bdrShadow.Visibility = Visibility.Visible;
        }

        void StopProcessing()
        {
            bdrShadow.Visibility = Visibility.Hidden;
            this.Cursor = Cursors.Arrow;
        }

        void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = filter.IsMatch(e.Item);
        }

        private void OpenFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !liveMode;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.AbsoluteUri);
                e.Handled = true;
            }
            catch { }            
        }

        private void ExportLogEntries_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            bool? result = saveDialog.ShowDialog();
            if (!result.Value)
                return;

            var exporter = new LogExporter();
            CollectionViewSource viewSource = GetCollectionViewSource();
            if (viewSource.View != null)
                exporter.Save(saveDialog.OpenFile(), viewSource.View.Cast<LogEntry>());
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Maximized = (WindowState == WindowState.Maximized);
        }

        private void lvCopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var logEntry = lstLog.SelectedItem as LogEntryViewModel;
            if (logEntry != null)
                Clipboard.SetText(LogExporter.Format(logEntry));
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsWindow settingsWin = new SettingsWindow();
            settingsWin.Owner = this;
            if (settingsWin.ShowDialog().Value)
                logEntries.MaxItems = Properties.Settings.Default.LiveLimit;
        }

        private void Previous_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bookmarkNavigator.Previous();
        }        

        private void Next_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bookmarkNavigator.Next();
        }        

        private void ToggleBookmark_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ToggleLogEntryBookmark();
        }

        private void ToogleBookmark_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstLog != null && lstLog.SelectedItem != null);
        }

        private void img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleLogEntryBookmark();
        }

        private void ToggleLogEntryBookmark()
        {
            LogEntryViewModel selected = lstLog.SelectedItem as LogEntryViewModel;
            if (selected != null)
                selected.Bookmarked = !selected.Bookmarked;
        }   

        private void Navigate_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = logEntries.Any(le => le.Bookmarked);
        }

        private void ClearLogs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = logEntries.Count > 0;
        }

        private void ClearLogs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Reset();
        }
    }
}
