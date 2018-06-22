using MyGitClient.Serivces;
using MyGitClient.View;
using MyGitClient.ViewModels;
using System;
using System.Windows;

namespace MyGitClient
{
    public partial class CommitWindow : Window
    {
        private Guid _repositoryId { get; set; }

        public CommitWindow(Guid id)
        {
            InitializeComponent();
            _repositoryId = id;
            DataContext = new CommitViewModel(_repositoryId);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void ShowBranchWindow(object sender, RoutedEventArgs e)
        {
            BranchWindow branchWindow = new BranchWindow(_repositoryId);
            branchWindow.Show();
        }

        private void ShowMergeWindow(object sender, RoutedEventArgs e)
        {
            MergeWindow mergeWindow = new MergeWindow(_repositoryId);
            mergeWindow.Show();
        }
    }
}
