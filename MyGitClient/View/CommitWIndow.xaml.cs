using MyGitClient.ViewModels;
using System;
using System.Windows;

namespace MyGitClient
{
    public partial class CommitWindow : Window
    {
        public CommitWindow(Guid id)
        {
            InitializeComponent();
            DataContext = new CommitViewModel(id);
        }
    }
}
