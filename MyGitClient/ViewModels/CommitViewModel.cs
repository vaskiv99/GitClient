﻿using MyGitClient.DTO;
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
using MyGitClient.View;

namespace MyGitClient.ViewModels
{
    public class CommitViewModel : INotifyPropertyChanged
    {
        #region Fields
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
        private readonly Guid _repositoryId;
        private Branch _branch;
        private Branch _selectedBranch;
        private readonly GitManager _gitManager;
        private readonly BranchService _branchService;
        private CommitService _commitService;
        private ObservableCollection<Branch> _branches;
        private ObservableCollection<CommitDto> _commits;
        private ObservableCollection<string> _changedFiles;
        private ObservableCollection<string> _stage;
        private readonly CommitWindow _commitWindow =
            Application.Current.Windows.OfType<CommitWindow>().FirstOrDefault();
        #endregion

        #region Commands
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
        private AsyncCommand _checkout;
        private AsyncCommand _back;
        private AsyncCommand _branchWindow;
        private AsyncCommand _mergeWindow;

        public AsyncCommand StageCommand => _stageCommand ?? (_stageCommand = new AsyncCommand(StageSelectedAsync));
        public AsyncCommand CommitCommand => _commitCommand ?? (_commitCommand = new AsyncCommand(CommitAsync));
        public AsyncCommand StageAllCommand => _stageAllCommand ?? (_stageAllCommand = new AsyncCommand(StageAllAsync));
        public AsyncCommand UnStageAllCommand => _unStageAllCommand ?? (_unStageAllCommand = new AsyncCommand(UnStageAllAsync));
        public AsyncCommand UnStageCommand => _unStageCommand ?? (_unStageCommand = new AsyncCommand(UnStageSelectedAsync));
        public AsyncCommand CreateBranch => _createBranch ?? (_createBranch = new AsyncCommand(CreateBranchAsync));
        public AsyncCommand DeleteBranch => _deleteBranch ?? (_deleteBranch = new AsyncCommand(DeleteBranchAsync));
        public AsyncCommand Push => _push ?? (_push = new AsyncCommand(PushAsync));
        public AsyncCommand Fetch => _fetch ?? (_fetch = new AsyncCommand(FetchAsync));
        public AsyncCommand Pull => _pull ?? (_pull = new AsyncCommand(PullAsync));
        public AsyncCommand Merge => _merge ?? (_merge = new AsyncCommand(MergeAsync));
        public AsyncCommand Checkout => _checkout ?? (_checkout = new AsyncCommand(CheckoutAsync));
        public AsyncCommand Back => _back ?? (_back = new AsyncCommand(BackToMainAsync));
        public AsyncCommand BranchWindow => _branchWindow ?? (_branchWindow = new AsyncCommand(ShowBranchWindowAsync));
        public AsyncCommand MergeWindow => _mergeWindow ?? (_mergeWindow = new AsyncCommand(ShowMergeWindowAsync));
        #endregion

        #region Properties
        public bool IsPush
        {
            get { return _isPush; }
            set
            {
                _isPush = value;
                OnPropertyChanged(nameof(IsPush));
            }
        }
        public Brush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
        public Brush ColorPush
        {
            get { return _colorPush; }
            set
            {
                _colorPush = value;
                OnPropertyChanged(nameof(ColorPush));
            }
        }
        public bool IsCheckout
        {
            get { return _isCheckout; }
            set
            {
                _isCheckout = value;
                OnPropertyChanged(nameof(IsCheckout));
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        public Branch Branch
        {
            get { return _branch; }
            set
            {
                _branch = value;
                OnPropertyChanged(nameof(Branch));
            }
        }
        public string SelectedChangeFiles
        {
            get { return _selectedChangeFiles; }
            set
            {
                _selectedChangeFiles = value;
                OnPropertyChanged(nameof(SelectedChangeFiles));
            }
        }
        public string SelectedStageFiles
        {
            get { return _selectedStageFiles; }
            set
            {
                _selectedStageFiles = value;
                OnPropertyChanged(nameof(SelectedStageFiles));
            }
        }
        public string NameBranch
        {
            get { return _nameBranch; }
            set
            {
                _nameBranch = value;
                OnPropertyChanged(nameof(NameBranch));
            }
        }
        public string HeadBranch
        {
            get { return _headBranch; }
            set
            {
                _headBranch = value;
                OnPropertyChanged(nameof(HeadBranch));
            }
        }
        public Branch SelectedBranch
        {
            get { return _selectedBranch; }
            set
            {
                _selectedBranch = value;
                OnPropertyChanged(nameof(SelectedBranch));
            }
        }
        public Visibility BarVisibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                OnPropertyChanged(nameof(BarVisibility));
            }
        }
        public Visibility MergeBarVisibility
        {
            get { return _mergeVisibility; }
            set
            {
                _mergeVisibility = value;
                OnPropertyChanged(nameof(MergeBarVisibility));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Branch> Branches
        {
            get { return _branches; }
            set
            {
                _branches = value;
                OnPropertyChanged(nameof(Branches));
            }
        }
        public ObservableCollection<CommitDto> Commits
        {
            get { return _commits; }
            set
            {
                _commits = value;
                OnPropertyChanged(nameof(Commits));
            }
        }
        public ObservableCollection<string> Files
        {
            get { return _changedFiles; }
            set
            {
                _changedFiles = value;
                OnPropertyChanged(nameof(Files));
            }
        }
        public ObservableCollection<string> Stage
        {
            get { return _stage; }
            set
            {
                _stage = value;
                OnPropertyChanged(nameof(Stage));
            }
        }
        #endregion

        #region Init
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
            Task.Run(async () =>
            {
                var result = await _gitManager.IsExistPullAsync(_repositoryId).ConfigureAwait(false);
                if (!result)
                    Color = Brushes.Red;
            });
        }
        #endregion

        #region Methods
        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private async Task StageSelectedAsync()
        {
            if (_selectedChangeFiles != null)
                return;
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    _stage.Add(_selectedChangeFiles);
                    _changedFiles.Remove(_selectedChangeFiles);
                });
            });

        }
        private async Task StageAllAsync()
        {
            if (_changedFiles.Count != 0)
                return;
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    _stage.AddRange(_changedFiles);
                    _changedFiles.Clear();
                });
            });
        }
        private async Task UnStageAllAsync()
        {
            if (_stage.Count != 0)
                return;
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    _changedFiles.AddRange(_stage);
                    _stage.Clear();
                });
            });
        }
        private async Task UnStageSelectedAsync()
        {
            if (_selectedStageFiles != null)
                return;
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    _changedFiles.Add(_selectedStageFiles);
                    _stage.Remove(_selectedStageFiles);
                });
            });

        }
        private async Task CommitAsync()
        {
            if (Stage.Count == 0 || string.IsNullOrWhiteSpace(Message))
                return;
            BarVisibility = Visibility.Visible;
            var commit = await _gitManager.GitCommitAsync(_message, _repositoryId, _stage);
            if (commit.Item1 == null)
            {
                MessageBox.Show(commit.Item2);
                BarVisibility = Visibility.Hidden;
                return;
            }
            if (IsPush)
                await _gitManager.GitPushAsync(_repositoryId);
            else
            {
                ColorPush = Brushes.Red;
            }
            Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                Commits.Insert(0, commit.Item1);
            });
            Stage.Clear();
            Message = null;
            BarVisibility = Visibility.Hidden;

        }
        private async Task CreateBranchAsync()
        {
            var result = await _gitManager.GitCreateBranchAsync(_repositoryId, _isCheckout, _nameBranch.ToLower()).ConfigureAwait(false);
            Application.Current.Dispatcher.Invoke(delegate
            {
                Branches.Add(result);
            });
            NameBranch = null;
        }
        private async Task DeleteBranchAsync()
        {
            await _gitManager.GitDeleteBranchAsync(_repositoryId, _selectedBranch.Id).ConfigureAwait(false);
            Application.Current.Dispatcher.Invoke(delegate
            {
                Branches.Remove(_selectedBranch);
            });
        }
        private async Task PushAsync()
        {
            BarVisibility = Visibility.Visible;
            var result = await _gitManager.GitPushAsync(_repositoryId);
            if (!string.IsNullOrWhiteSpace(result))
            {
                MessageBox.Show(result);
                BarVisibility = Visibility.Hidden;
                return;
            }
            ColorPush = Brushes.Blue;
            BarVisibility = Visibility.Hidden;
        }
        private async Task FetchAsync()
        {
            BarVisibility = Visibility.Visible;
            var result = await _gitManager.GitFetchAsync(_repositoryId);
            if (!string.IsNullOrWhiteSpace(result.Item2))
            {
                MessageBox.Show(result.Item2);
                BarVisibility = Visibility.Hidden;
                return;
            }
            if (result.Item1)
                Color = Brushes.Red;
            BarVisibility = Visibility.Hidden;
        }
        private async Task PullAsync()
        {
            BarVisibility = Visibility.Visible;
            var result = await _gitManager.GitPullAsync(_repositoryId).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(result))
            {
                MessageBox.Show(result);
                BarVisibility = Visibility.Hidden;
                return;
            }
            BarVisibility = Visibility.Hidden;
        }
        private async Task MergeAsync()
        {
            MergeBarVisibility = Visibility.Visible;
            var result = await _gitManager.GitMerge(_repositoryId, _selectedBranch.Id).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(result))
            {
                MessageBox.Show(result);
                MergeBarVisibility = Visibility.Hidden;
                return;
            }
            ColorPush = Brushes.Red;
            MergeBarVisibility = Visibility.Hidden;
        }
        private async Task CheckoutAsync()
        {
            if (_selectedBranch != null)
            {
                await _gitManager.GitCheckoutAsync(_repositoryId, _selectedBranch.Id).ConfigureAwait(false);
                HeadBranch = _selectedBranch.Name;
                var result = await _gitManager.IsExistPullAsync(_repositoryId).ConfigureAwait(false);
                if (!result)
                {
                    Color = Brushes.Red;
                }
                else
                {
                    Color = Brushes.Blue;
                }
            }
        }
        private async Task BackToMainAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MainWindow mainWindow = new MainWindow();
                    _commitWindow.Close();
                    mainWindow.Show();
                });
            });
        }
        private async Task ShowBranchWindowAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    BranchWindow branchWindow = new BranchWindow(_repositoryId);
                    branchWindow.ShowDialog();
                });
            });
        }
        private async Task ShowMergeWindowAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MergeWindow mergeWindow = new MergeWindow(_repositoryId);
                    mergeWindow.ShowDialog();
                });
            });
        }
        #endregion
    }
}
