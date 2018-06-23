using MyGitClient.DTO;
using MyGitClient.Helpers;
using MyGitClient.Serivces;
using Nito.Mvvm;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using MyGitClient.Models;
using System.Windows.Media;

namespace MyGitClient.ViewModels
{
    public class CommitViewModel : INotifyPropertyChanged
    {
        private string _message;
        private string _headBranch;
        private Brush _color;
        private Brush _colorPush;
        private bool _isPush;
        private Visibility _visibility;
        private Visibility _mergeVisibility;
        private string _selectedChangeFiles;
        private string _selectedStageFiles;
        private string _nameBranch;
        private bool _isCheckout;
        private Guid _repositoryId;
        private Branch _branch;
        private Branch _selectedBranch;
        private GitManager _gitManager;
        private BranchService _branchService;
        private CommitService _commitService;
        private AsyncCommand _pull;
        private AsyncCommand _fetch;
        private AsyncCommand _push;
        private AsyncCommand _stageCommand;
        private AsyncCommand _commitCommand;
        private AsyncCommand _stageAllCommand;
        private AsyncCommand _unStageAllCommand;
        private AsyncCommand _unStageCommand;
        private AsyncCommand _createBranch;
        private AsyncCommand _deleteBranch;
        private AsyncCommand _merge;
        private ObservableCollection<Branch> _branches;
        private ObservableCollection<CommitDto> _commits;
        private ObservableCollection<string> _changedFiles;
        private ObservableCollection<string> _stage;
        private CommitWindow _commitWindow =
            Application.Current.Windows.OfType<CommitWindow>().FirstOrDefault();

        public bool IsPush
        {
            get { return _isPush; }
            set
            {
                _isPush = value;
                OnPropertyChanged("IsPush");
            }
        }
        public Brush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }
        public Brush ColorPush
        {
            get { return _colorPush; }
            set
            {
                _colorPush = value;
                OnPropertyChanged("ColorPush");
            }
        }
        public bool IsCheckout
        {
            get { return _isCheckout; }
            set
            {
                _isCheckout = value;
                OnPropertyChanged("IsCheckout");
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
        public Branch Branch
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
        public string NameBranch
        {
            get { return _nameBranch; }
            set
            {
                _nameBranch = value;
                OnPropertyChanged("NameBranch");
            }
        }
        public string HeadBranch
        {
            get { return _headBranch; }
            set
            {
                _headBranch = value;
                OnPropertyChanged("HeadBranch");
            }
        }
        public Branch SelectedBranch
        {
            get { return _selectedBranch; }
            set
            {
                _selectedBranch = value;
                OnPropertyChanged("SelectedBranch");
            }
        }
        public Visibility BarVisibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                OnPropertyChanged("BarVisibility");
            }
        }
        public Visibility MergeBarVisibility
        {
            get { return _mergeVisibility; }
            set
            {
                _mergeVisibility = value;
                OnPropertyChanged("MergeBarVisibility");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public AsyncCommand StageCommand
        {
            get
            {
                return _stageCommand ?? (_stageCommand = new AsyncCommand(StageSelectedAsync));
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
        public AsyncCommand CreateBranch
        {
            get
            {
                return _createBranch ?? (_createBranch = new AsyncCommand(CreateBranchAsync));
            }
        }
        public AsyncCommand DeleteBranch
        {
            get
            {
                return _deleteBranch ?? (_deleteBranch = new AsyncCommand(DeleteBranchAsync));
            }
        }
        public AsyncCommand Push
        {
            get
            {
                return _push ?? (_push = new AsyncCommand(PushAsync));
            }
        }
        public AsyncCommand Fetch
        {
            get
            {
                return _fetch ?? (_fetch = new AsyncCommand(FetchAsync));
            }
        }
        public AsyncCommand Pull
        {
            get
            {
                return _pull ?? (_pull = new AsyncCommand(PullAsync));
            }
        }
        public AsyncCommand Merge
        {
            get
            {
                return _merge ?? (_merge = new AsyncCommand(MergeAsync));
            }
        }
        public ObservableCollection<Branch> Branches
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
            _gitManager = new GitManager();
            _branchService = new BranchService();
            _commitService = new CommitService();
            _branches = new ObservableCollection<Branch>(_branchService.GetBranches(_repositoryId));
            _commits = new ObservableCollection<CommitDto>(_branchService.GetBranches(_repositoryId).CreateCommitsDto());
            _changedFiles = new ObservableCollection<string>(_gitManager.GitStatusAsync(_repositoryId).Result);
            _stage = new ObservableCollection<string>();
            _visibility = Visibility.Hidden;
            _mergeVisibility = Visibility.Hidden;
            Color = Brushes.Blue;
            ColorPush = Brushes.Blue;
            Task.Run(async () =>
            {
                HeadBranch = await _gitManager.NameHeadBranch(_repositoryId);
            });
        }

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private async Task StageSelectedAsync()
        {
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
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
                App.Current.Dispatcher.Invoke(delegate
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
            if (Stage.Count != 0 && !string.IsNullOrWhiteSpace(Message))
            {
                BarVisibility = Visibility.Visible;
                var commit = await _gitManager.GitCommitAsync(_message, _repositoryId, _stage);
                if (IsPush)
                    await _gitManager.GitPushAsync(_repositoryId);
                else
                {
                    ColorPush = Brushes.Red;
                }
                App.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    Commits.Insert(0, commit);
                });
                Stage.Clear();
                Message = null;
                BarVisibility = Visibility.Hidden;
            }
        }
        private async Task CreateBranchAsync()
        {
            var result = await _gitManager.GitCreateBranchAsync(_repositoryId, _isCheckout, _nameBranch.ToLower()).ConfigureAwait(false);
            App.Current.Dispatcher.Invoke(delegate
            {
                Branches.Add(result);
            });
            NameBranch = null;
        }
        private async Task DeleteBranchAsync()
        {
            await _gitManager.GitDeleteBranchAsync(_repositoryId, _selectedBranch.Id).ConfigureAwait(false);
            App.Current.Dispatcher.Invoke(delegate
            {
                Branches.Remove(_selectedBranch);
            });
        }
        private async Task PushAsync()
        {
            BarVisibility = Visibility.Visible;
            await _gitManager.GitPushAsync(_repositoryId);
            ColorPush = Brushes.Blue;
            BarVisibility = Visibility.Hidden;
        }
        private async Task FetchAsync()
        {
            BarVisibility = Visibility.Visible;
            var result = await _gitManager.GitFetchAsync(_repositoryId);
            if (result)
                Color = Brushes.Red;
            BarVisibility = Visibility.Hidden;
        }
        private async Task PullAsync()
        {
            BarVisibility = Visibility.Visible;
            var result = await _gitManager.GitPullAsync(_repositoryId).ConfigureAwait(false);
            if (result.Item2)
            {
                MessageBox.Show(result.Item1);
                Color = Brushes.Blue;
            }
            else
            {
                MessageBox.Show(result.Item1);
            }
            BarVisibility = Visibility.Hidden;
        }
        private async Task MergeAsync()
        {
            MergeBarVisibility = Visibility.Visible;
            var result = await _gitManager.GitMerge(_repositoryId, _selectedBranch.Id).ConfigureAwait(false);
            if (result.Item2)
            {
                MessageBox.Show(result.Item1);
                ColorPush = Brushes.Red;
            }
            else
            {
                MessageBox.Show(result.Item1);
            }
            MergeBarVisibility = Visibility.Hidden;
        }
    }
}
