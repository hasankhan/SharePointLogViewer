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
        BackgroundWorker worker;

        public static RoutedUICommand About = new RoutedUICommand("About", "About", typeof(MainWindow));
        public static RoutedUICommand Filter = new RoutedUICommand("Filter", "Filter", typeof(MainWindow));
        public static RoutedUICommand OpenFile = new RoutedUICommand("OpenFile", "OpenFile", typeof(MainWindow));

        public MainWindow()
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            InitializeComponent();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DataContext = null;
            this.DataContext = logEntries;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            logEntries.Clear();
            string[] files = e.Argument as string[];
            foreach (string file in files)
                logEntries.AddRange(LogParser.PraseLog(file));            
        }

        private void OpenFileExecuted(object sender, ExecutedRoutedEventArgs e)
        {            
            var dialog = new OpenFileDialog();
            dialog.Filter = "Log Files (*.log)|*.log";
            dialog.Multiselect = true;
            if (dialog.ShowDialog().Value)
                LoadFiles(dialog.FileNames);
        }

        private void LoadFiles(string[] files)
        {
            worker.RunWorkerAsync(files);
        }

        private void FilterExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string text = txtFilter.Text.ToLower().Trim();
            string filterBy = cmbFilterBy.Text;
            if (text == String.Empty)
                this.DataContext = logEntries;
            else
            {                
                Predicate<LogEntry> predicate;
                if (cmbFilterBy.SelectedIndex == 0)
                {
                    var predicates = from property in typeof(LogEntry).GetProperties()
                                     select CreatePredicate(property.Name, text);
                    predicate = entry => predicates.Any(p => p(entry));
                }
                else
                    predicate = CreatePredicate(filterBy, text);               
                List<LogEntry> filteredEntries = logEntries.FindAll(predicate);
                this.DataContext = filteredEntries;
            }
        }

        private static Predicate<LogEntry> CreatePredicate(string propertyName, string text)
        {
            Predicate<LogEntry> predicate;
            predicate = delegate(LogEntry entry)
            {
                PropertyInfo property = typeof(LogEntry).GetProperty(propertyName);
                if (property == null)
                    return false;
                string value = property.GetValue(entry, null) as String;
                if (value == null)
                    return false;
                return value.ToLower().Contains(text);
            };
            return predicate;
        }

        private void AboutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Copyright 2010 Overroot Inc.\n\nhttp://www.overroot.com", "About SharePointLogViewer", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {            
            string[] files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            e.Handled = true;
            LoadFiles(files);
        }
    }
}
