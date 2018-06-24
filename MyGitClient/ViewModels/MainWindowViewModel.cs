using Microsoft.WindowsAPICodePack.Dialogs;
using MyGitClient.Serivces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nito.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using MyGitClient.Helpers;
using System;

namespace MyGitClient.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields
        private RepositoriesService _repositoryService;
        private GitManager _gitManager;
        private string _url;
        private string _path;
        private string _name;
        private AsyncCommand _addRepository;
        private AsyncCommand _cloneCommand;
        private AsyncCommand _deleteCommand;
        private AsyncCommand _commitWindow;
        private Models.Repository _selectedRepository;
        private ObservableCollection<Models.Repository> _repositories;
        private MainWindow _mainWindow =
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        #endregion

        #region Properties
        public string URL
        {
            get { return _url; }
            set
            {
                _url = value;
                Name = CreateNameForRepositoryHelper.CreateName(_url);
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
        public AsyncCommand AddRepository
        {
            get
            {
                return _addRepository ?? (_addRepository = new AsyncCommand(AddExistingRepoAsync));
            }           
        }
        public RelayCommand BrowseCommand { get; set; }
        public AsyncCommand GoToCommitWindow
        {
            get
            {
                return _commitWindow ?? (_commitWindow = new AsyncCommand(ChangeWindow));
            }
        }
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
        #endregion

        #region Init
        public MainWindowViewModel()
        {
            _gitManager = new GitManager();
            _repositoryService = new RepositoriesService();
            _repositories = new ObservableCollection<Models.Repository>(_repositoryService.GetRepositories());
            BrowseCommand = new RelayCommand(SelectPath);
        }
        #endregion

        #region Methods
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
            var result = await _gitManager.CloneAsync(_url, _path, _name).ConfigureAwait(false);
            App.Current.Dispatcher.Invoke(delegate
            {
                _repositories.Add(result);
            });
        }
        private async Task AddExistingRepoAsync()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path = dialog.FileName;
            }
            var result = await _gitManager.AddExistingRepositoryAsync(_path).ConfigureAwait(false);
            App.Current.Dispatcher.Invoke(delegate
            {
                _repositories.Add(result);
            });
        }
        private async Task DeleteAsync()
        {
            await _repositoryService.DeleteRepositoryAsync(_selectedRepository.Id).ConfigureAwait(false);
            App.Current.Dispatcher.Invoke(delegate
            {
                _repositories.Remove(_selectedRepository);
            });
        }
        private async Task ChangeWindow()
        {
            if (_selectedRepository != null)
            {
               await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate {
                        CommitWindow commitWindow = new CommitWindow(_selectedRepository.Id);
                        _mainWindow.Close();
                        commitWindow.Show();
                    });
                });
            }
        }
        #endregion
    }
}
