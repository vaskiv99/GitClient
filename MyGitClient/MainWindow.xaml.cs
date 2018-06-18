using System.Windows;
using System.Windows.Input;
using MyGitClient.ViewModels;
namespace MyGitClient
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindow;

        public MainWindow()
        {
            _mainWindow = new MainWindowViewModel();
            InitializeComponent();
            DataContext = _mainWindow;
        }

        private void ListRepositories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Models.Repository repository = (Models.Repository)ListRepositories.SelectedItem;
            CommitWindow commit = new CommitWindow(repository.Id);
            commit.Show();
            Hide();
        }
    }
}
