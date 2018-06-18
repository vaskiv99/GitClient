using Microsoft.WindowsAPICodePack.Dialogs;
using MyGitClient.Serivces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nito.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace MyGitClient.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private RepositoriesService _repositoryService;
        private GitService _gitService;
        private string _url;
        private string _path;
        private string _name;
        private AsyncCommand _cloneCommand;
        private RelayCommand _repositoryWindowCommand;
        private AsyncCommand _deleteCommand;
        private Models.Repository _selectedRepository;
        private ObservableCollection<Models.Repository> _repositories;
        private MainWindow _mainWindow =
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        public string URL
        {
            get { return _url; }
            set
            {
                _url = value;
                Name = _gitService.CreateName(_url);
                OnPropertyChanged("URL");
            }
        }
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public AsyncCommand DeleteRepository
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new AsyncCommand(DeleteAsync));
            }
        }
        public AsyncCommand CloneCommand
        {
            get
            {
                return _cloneCommand ?? (_cloneCommand = new AsyncCommand(CloneAsync));
            }
        }
        public RelayCommand RepositoryWindowCommand
        {
            get
            {
                return _repositoryWindowCommand ?? (_repositoryWindowCommand = new RelayCommand(ChangeWindow));
            }
        }
        public RelayCommand BrowseCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Models.Repository> Repositories
        {
            get { return _repositories; }
            set
            {
                _repositories = value;
                OnPropertyChanged("Repositories");
            }
        }
        public Models.Repository SelectedRepository
        {
            get { return _selectedRepository; }
            set
            {
                _selectedRepository = value;
                OnPropertyChanged("SelectedRepository");
            }
        }
        
        public MainWindowViewModel()
        {
            _gitService = new GitService();
            _repositoryService = new RepositoriesService();
            _repositories = new ObservableCollection<Models.Repository>(_repositoryService.GetRepositories());
            BrowseCommand = new RelayCommand(SelectPath);
        }

        private void ChangeWindow(object parametr)
        {
            CommitWindow commit = new CommitWindow(_selectedRepository.Id);
            commit.Show();
        }
        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private void SelectPath(object parametr)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path = dialog.FileName;
            }
        }
        private async Task CloneAsync()
        {
            _path = _path + $@"\{_name}";
            var result = await _gitService.CloneAsync(_url, _path, _name).ConfigureAwait(false);
            App.Current.Dispatcher.Invoke((System.Action)delegate
            {
                _repositories.Add(result);
            });
        }
        private async Task DeleteAsync()
        {
            await _repositoryService.DeleteRepositoryAsync(_selectedRepository.Id).ConfigureAwait(false);
            App.Current.Dispatcher.Invoke((System.Action)delegate
            {
                _repositories.Remove(_selectedRepository);
            });
        }
    }
}
