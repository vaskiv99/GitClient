using MyGitClient.ViewModels;
using System;
using System.Windows;

namespace MyGitClient.View
{
    public partial class MergeWindow : Window
    {
        public MergeWindow(Guid repositoryId)
        {
            InitializeComponent();
            DataContext = new CommitViewModel(repositoryId);
        }
    }
}
