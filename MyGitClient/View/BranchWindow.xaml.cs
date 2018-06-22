using MyGitClient.ViewModels;
using System;
using System.Windows;

namespace MyGitClient.View
{
    public partial class BranchWindow : Window
    {
        public BranchWindow(Guid repositoryId)
        {
            InitializeComponent();
            DataContext = new CommitViewModel(repositoryId);
        }
    }
}
