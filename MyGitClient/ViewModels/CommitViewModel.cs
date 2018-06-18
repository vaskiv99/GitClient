using LibGit2Sharp;
using MyGitClient.DTO;
using MyGitClient.Helpers;
using MyGitClient.Serivces;
using Nito.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MyGitClient.ViewModels
{
    public class CommitViewModel : INotifyPropertyChanged
    {
        private string _message;
        private bool _isPush;
        private string _selectedChangeFiles;
        private string _selectedStageFiles;
        private Guid _repositoryId;
        private Models.Branch _branch;
        private GitService _gitService;
        private BranchService _branchService;
        private CommitService _commitService;
        private AsyncCommand _stageCommand;
        private AsyncCommand _commitCommand;
        private AsyncCommand _stageAllCommand;
        private AsyncCommand _unStageAllCommand;
        private AsyncCommand _unStageCommand;
        private ObservableCollection<Models.Branch> _branches;
        private ObservableCollection<CommitDto> _commits;
        private ObservableCollection<string> _changedFiles;
        private ObservableCollection<string> _stage;

        public bool IsPush
        {
            get { return _isPush; }
            set
            {
                _isPush = value;
                OnPropertyChanged("IsPush");
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }
        public Models.Branch Branch
        {
            get { return _branch; }
            set
            {
                _branch = value;
                OnPropertyChanged("Branch");
            }
        }
        public string SelectedChangeFiles
        {
            get { return _selectedChangeFiles; }
            set
            {
                _selectedChangeFiles = value;
                OnPropertyChanged("SelectedChangeFiles");
            }
        }
        public string SelectedStageFiles
        {
            get { return _selectedStageFiles; }
            set
            {
                _selectedStageFiles = value;
                OnPropertyChanged("SelectedStageFiles");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public AsyncCommand StageCommand
        {
            get
            {
                return _stageCommand ?? (_stageCommand = new AsyncCommand(StageAllAsync));
            }
        }
        public AsyncCommand CommitCommand
        {
            get
            {
                return _commitCommand ?? (_commitCommand = new AsyncCommand(CommitAsync));
            }
        }
        public AsyncCommand StageAllCommand
        {
            get
            {
                return _stageAllCommand ?? (_stageAllCommand = new AsyncCommand(StageAllAsync));
            }
        }
        public AsyncCommand UnStageAllCommand
        {
            get
            {
                return _unStageAllCommand ?? (_unStageAllCommand = new AsyncCommand(UnStageAllAsync));
            }
        }
        public AsyncCommand UnStageCommand
        {
            get
            {
                return _unStageCommand ?? (_unStageCommand = new AsyncCommand(UnStageSelectedAsync));
            }
        }
        public ObservableCollection<Models.Branch> Branches
        {
            get { return _branches; }
            set
            {
                _branches = value;
                OnPropertyChanged("Branches");
            }
        }
        public ObservableCollection<CommitDto> Commits
        {
            get { return _commits; }
            set
            {
                _commits = value;
                OnPropertyChanged("Commits");
            }
        }
        public ObservableCollection<string> Files
        {
            get { return _changedFiles; }
            set
            {
                _changedFiles = value;
                OnPropertyChanged("Files");
            }
        }
        public ObservableCollection<string> Stage
        {
            get { return _stage; }
            set
            {
                _stage = value;
                OnPropertyChanged("Stage");
            }
        }

        public CommitViewModel(Guid repositoryId)
        {
            _repositoryId = repositoryId;
            _gitService = new GitService();
            _branchService = new BranchService();
            _commitService = new CommitService();
            _branches = new ObservableCollection<Models.Branch>(_branchService.GetBranches(_repositoryId));
            _commits = new ObservableCollection<CommitDto>(_branchService.GetBranches(_repositoryId).CreateCommitsDto());
            _changedFiles = new ObservableCollection<string>(_gitService.GitStatusAsync(_repositoryId).Result);
            _stage = new ObservableCollection<string>();
        }

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private async Task StageSelectedAsync()
        {
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    _stage.Add(_selectedChangeFiles);
                    _changedFiles.Remove(_selectedChangeFiles);
                });
            });
        }
        private async Task StageAllAsync()
        {
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    _stage.AddRange(_changedFiles);
                    _changedFiles.Clear();
                });
            });
        }
        private async Task UnStageAllAsync()
        {
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    _changedFiles.AddRange(_stage);
                    _stage.Clear();
                });
            });
        }
        private async Task UnStageSelectedAsync()
        {
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    _changedFiles.Add(_selectedStageFiles);
                    _stage.Remove(_selectedStageFiles);
                });
            });
        }
        private async Task CommitAsync()
        {
            var commit = await _gitService.GitCommitAsync(_message, _repositoryId, _stage);
            if (IsPush)
                await _gitService.GitPushAsync(_repositoryId);
            App.Current.Dispatcher.Invoke((System.Action)delegate
            {
                _commits.Add(commit);
            });
            _stage.Clear();
            _message = "";
        }
    }

}
