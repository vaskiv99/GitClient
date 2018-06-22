using MyGitClient.ViewModels;
using System;
using System.Windows;

namespace MyGitClient.View
{
    /// <summary>
    /// Interaction logic for MergeWindow.xaml
    /// </summary>
    public partial class MergeWindow : Window
    {
        public MergeWindow(Guid repositoryId)
        {
            InitializeComponent();
            DataContext = new CommitViewModel(repositoryId);
        }
    }
}
