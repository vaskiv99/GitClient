using MyGitClient.Serivces;
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
    }
}
