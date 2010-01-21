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
                switch (filterBy)
                {
                    case "Timestamp":
                        predicate = entry => entry.Timestamp.ToLower().Contains(text); break;                        
                    case "Process":
                        predicate = entry => entry.Process.ToLower().Contains(text); break;
                    case "TID":
                        predicate = entry => entry.TID.ToLower().Contains(text); break;
                    case "Area":
                        predicate = entry => entry.Area.ToLower().Contains(text); break;
                    case "Category":
                        predicate = entry => entry.Category.ToLower().Contains(text); break;
                    case "EventID":
                        predicate = entry => entry.EventID.ToLower().Contains(text); break;
                    case "Level":
                        predicate = entry => entry.Level.ToLower().Contains(text); break;
                    case "Message":
                        predicate = entry => entry.Message.ToLower().Contains(text); break;
                    case "Correlation":
                    default:
                        predicate = entry => entry.Correlation.ToLower().Contains(text); break;
                }
                List<LogEntry> filteredEntries = logEntries.FindAll(predicate);
                this.DataContext = filteredEntries;
            }
        }

        private void AboutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Copyright 2010 Overroot Inc.", "About NCachePoint", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {            
            string[] files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            e.Handled = true;
            LoadFiles(files);
        }
    }
}
