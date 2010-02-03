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

namespace SharePointLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<LogEntry> logEntries = new ObservableCollection<LogEntry>();
        LogsLoader logsLoader = new LogsLoader();
        LogMonitor watcher = null;
        DynamicFilter filter;
        bool liveMode;

        string[] files = new string[0];

        public static RoutedUICommand About = new RoutedUICommand("About", "About", typeof(MainWindow));
        public static RoutedUICommand Filter = new RoutedUICommand("Filter", "Filter", typeof(MainWindow));
        public static RoutedUICommand Refresh = new RoutedUICommand("Refresh", "Refresh", typeof(MainWindow));
        public static RoutedUICommand OpenFile = new RoutedUICommand("OpenFile", "OpenFile", typeof(MainWindow));
        public static RoutedUICommand Live = new RoutedUICommand("Live", "Live", typeof(MainWindow));
        public static RoutedUICommand Offline = new RoutedUICommand("Offline", "Offline", typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
            logsLoader.LoadCompleted += new EventHandler<LoadCompletedEventArgs>(logsLoader_LoadCompleted);
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFilter();
        }

        void logsLoader_LoadCompleted(object sender, LoadCompletedEventArgs e)
        {
            logEntries = new ObservableCollection<LogEntry>(e.LogEntries);
            UpdateFilter();
            this.DataContext = logEntries;
            StopProcessing();
        }

        void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            var dialog = new OpenFileDialog();
            dialog.Filter = "Log Files (*.log)|*.log";
            dialog.Multiselect = true;
            if (dialog.ShowDialog().Value)
            {
                files = dialog.FileNames;
                LoadFiles();
            }
        }

        void FilterExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            string criteria = cmbFilterBy.Text == "Any field" ? "*" : cmbFilterBy.Text;
            filter = DynamicFilter.Create<LogEntry>(criteria, txtFilter.Text);
            CollectionViewSource source = (CollectionViewSource)this.Resources["FilteredCollection"];
            if(source.View != null)
                source.View.Refresh();

        }

        void AboutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Copyright 2010 Overroot Inc.\n\nWebsite: http://www.overroot.com\nEmail: overrootinc@gmail.com", "About SharePointLogViewer", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (!SPUtility.IsMOSSInstalled)
            {
                MessageBox.Show("Microsoft Sharepoint not installed on this machine");
                return;
            }
            string folderPath = SPUtility.LogsLocations;

            watcher = new LogMonitor(folderPath);
            watcher.LogEntryDiscovered += new EventHandler<LogEntryDiscoveredEventArgs>(watcher_LogEntryDiscovered);

            ChangeMode(true);
            logEntries.Clear();
            this.DataContext = logEntries;

            watcher.Start();
        }

        void watcher_LogEntryDiscovered(object sender, LogEntryDiscoveredEventArgs e)
        {
            Dispatcher.Invoke((Action)(() => logEntries.Add(e.LogEntry)));
        }

        void OfflineMode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (watcher != null)
            {
                watcher.Stop();
                watcher.Dispose();
                watcher = null;
            }

            ChangeMode(false);
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

        void ChangeMode(bool live)
        {
            btnOffline.Visibility = (live ? Visibility.Visible : Visibility.Hidden);
            btnLive.Visibility = (live ? Visibility.Hidden : Visibility.Visible);

            liveMode = live;
        }

        void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = filter.IsMatch(e.Item);
        }

        private void OpenFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !liveMode;
        }
    }
}
