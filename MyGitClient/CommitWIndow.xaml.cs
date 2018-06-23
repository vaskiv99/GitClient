using MyGitClient.Serivces;
using MyGitClient.View;
using MyGitClient.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MyGitClient
{
    public partial class CommitWindow : Window
    {
        private Guid _repositoryId { get; set; }
        private GitManager _gitManager;
        public CommitWindow(Guid id)
        {
            InitializeComponent();
            _repositoryId = id;
            _gitManager = new GitManager();
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

        private void branches_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Models.Branch branch = (Models.Branch)branches.SelectedItem;
            Task.Run(async () =>
            {
                await _gitManager.GitCheckoutAsync(_repositoryId, branch.Id);
            });
            HeadBranch.Content = branch.Name;
        }
    }
}
