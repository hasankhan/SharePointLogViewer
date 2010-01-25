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

namespace SharePointLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<LogEntry> logEntries = new List<LogEntry>();
        LogsLoader logsLoader = new LogsLoader();
        ListSearcher<LogEntry> listSearcher = new ListSearcher<LogEntry>();
        string[] files = new string[0];

        public static RoutedUICommand About = new RoutedUICommand("About", "About", typeof(MainWindow));
        public static RoutedUICommand Filter = new RoutedUICommand("Filter", "Filter", typeof(MainWindow));
        public static RoutedUICommand Refresh = new RoutedUICommand("Refresh", "Refresh", typeof(MainWindow));
        public static RoutedUICommand OpenFile = new RoutedUICommand("OpenFile", "OpenFile", typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
            logsLoader.LoadCompleted += new EventHandler<LoadCompletedEventArgs>(logsLoader_LoadCompleted);
            listSearcher.SearchComplete += new EventHandler<ListSearchCompleteEventArgs<LogEntry>>(listSearcher_SearchComplete);
        }

        void logsLoader_LoadCompleted(object sender, LoadCompletedEventArgs e)
        {
            this.DataContext = null;
            logEntries = (List<LogEntry>)e.LogEntries;
            this.DataContext = logEntries;
            StopProcessing();
            if (txtFilter.Text != String.Empty)
                Search(txtFilter.Text);
        }

        void OpenFileExecuted(object sender, ExecutedRoutedEventArgs e)
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
            string text = txtFilter.Text.ToLower().Trim();
            string filterBy = cmbFilterBy.Text;
            if (text == String.Empty)
                this.DataContext = logEntries;
            else
                Search(text);
        }

        void Search(string text)
        {
            string criterea = cmbFilterBy.SelectedIndex == 0 ? "*" : cmbFilterBy.Text;
            listSearcher.Start(logEntries, criterea, text);
            StartProcessing("Searching...");
        }

        void listSearcher_SearchComplete(object sender, ListSearchCompleteEventArgs<LogEntry> e)
        {
            this.DataContext = e.Result;
            StopProcessing();
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
            e.CanExecute = files.Length > 0;
        }

        void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadFiles();
        }

        void LoadFiles()
        {
            logsLoader.Start(files);
            StartProcessing("Loading...");
        }

        void StartProcessing(string message)
        {
            statusMessage.Text = message;
            statusMessage.Visibility = Visibility.Visible;
            this.Cursor = Cursors.Wait;
        }

        void StopProcessing()
        {
            statusMessage.Visibility = Visibility.Hidden;
            this.Cursor = Cursors.Arrow;
        }
    }
}
