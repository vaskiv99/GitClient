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
    }
}
