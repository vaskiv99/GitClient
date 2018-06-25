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
using MyGitClient.Models;

namespace MyGitClient.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields
        private readonly RepositoriesService _repositoryService;
        private readonly GitManager _gitManager;
        private string _url;
        private string _path;
        private string _name;

        private Models.Repository _selectedRepository;
        private ObservableCollection<Models.Repository> _repositories;
        private readonly MainWindow _mainWindow =
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        #endregion

        #region Commands
        private AsyncCommand _addRepository;
        private AsyncCommand _cloneCommand;
        private AsyncCommand _deleteCommand;
        private AsyncCommand _commitWindow;

        public AsyncCommand DeleteRepository => _deleteCommand ?? (_deleteCommand = new AsyncCommand(DeleteAsync));
        public AsyncCommand CloneCommand => _cloneCommand ?? (_cloneCommand = new AsyncCommand(CloneAsync));
        public AsyncCommand AddRepository => _addRepository ?? (_addRepository = new AsyncCommand(AddExistingRepoAsync));
        public RelayCommand BrowseCommand { get; set; }
        public AsyncCommand GoToCommitWindow => _commitWindow ?? (_commitWindow = new AsyncCommand(ChangeWindow));
        #endregion

        #region Properties
        public string URL
        {
            get { return _url; }
            set
            {
                _url = value;
                Name = CreateNameForRepositoryHelper.CreateName(_url);
                OnPropertyChanged(nameof(URL));
            }
        }
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Models.Repository> Repositories
        {
            get { return _repositories; }
            set
            {
                _repositories = value;
                OnPropertyChanged(nameof(Repositories));
            }
        }
        public Models.Repository SelectedRepository
        {
            get { return _selectedRepository; }
            set
            {
                _selectedRepository = value;
                OnPropertyChanged(nameof(SelectedRepository));
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
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path = dialog.FileName;
            }
        }
        private async Task CloneAsync()
        {
            if (string.IsNullOrWhiteSpace(_url) || string.IsNullOrWhiteSpace(_path))
                return;
            var repository = new Repository();
            var error = string.Empty;
            try
            {
                var result = await _gitManager.CloneAsync(_url, _path, _name).ConfigureAwait(false);
                repository = result.Item1;
                error = result.Item2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (repository == null)
            {
                MessageBox.Show(error);
                return;
            }
            Application.Current.Dispatcher.Invoke(delegate
            {
                _repositories.Add(repository);
            });
            URL = null;
            Path = null;
            Name = null;
        }
        private async Task AddExistingRepoAsync()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path = dialog.FileName;
            }
            var error = string.Empty;
            var repository = new Repository();
            try
            {
                var result = await _gitManager.AddExistingRepositoryAsync(_path).ConfigureAwait(false);
                error = result.Item2;
                repository = result.Item1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (repository == null)
            {
                MessageBox.Show(error);
                return;
            }
            Application.Current.Dispatcher.Invoke(delegate
            {
                _repositories.Add(repository);
            });
            Path = null;
        }
        private async Task DeleteAsync()
        {
            if (_selectedRepository == null)
                return;
            await _repositoryService.DeleteRepositoryAsync(_selectedRepository.Id).ConfigureAwait(false);
            Application.Current.Dispatcher.Invoke(delegate
            {
                _repositories.Remove(_selectedRepository);
            });
        }
        private async Task ChangeWindow()
        {
            if (_selectedRepository == null)
                return;
            await Task.Run(() =>
             {
                 Application.Current.Dispatcher.Invoke((Action)delegate
                 {
                     CommitWindow commitWindow = new CommitWindow(_selectedRepository.Id);
                     _mainWindow.Close();
                     commitWindow.Show();
                 });
             });
        }
        #endregion
    }
}
