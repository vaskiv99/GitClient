using System.Windows;
using MyGitClient.ViewModels;
namespace MyGitClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
