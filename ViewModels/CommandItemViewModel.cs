using MaterialDesignThemes.Wpf;
using ScriptLauncher.Models;
using ScriptLauncher.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ScriptLauncher.ViewModels
{
    public class CommandItemViewModel : ViewModelBase
    {
        private readonly CommandExecutor _executor;
        private CancellationTokenSource _highlightResetCts;
        public CommandItem Item { get; }

        public string Name => Item.Name;
        public string Description => Item.Description;
        public bool RunAsAdministrator => Item.RunAsAdministrator;

        public PackIconKind IconKind
        {
            get
            {
                if (Enum.TryParse<PackIconKind>(Item.Icon, out var kind))
                    return kind;
                return PackIconKind.Console;
            }
        }

        public ICommand ExecuteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        private bool _hasRecentExecution;
        public bool HasRecentExecution
        {
            get => _hasRecentExecution;
            private set
            {
                _hasRecentExecution = value;
                OnPropertyChanged();
            }
        }

        private bool _lastExecutionSuccess;
        public bool LastExecutionSuccess
        {
            get => _lastExecutionSuccess;
            private set
            {
                _lastExecutionSuccess = value;
                OnPropertyChanged();
            }
        }

        public event Action<CommandItemViewModel> OnEditRequested;
        public event Action<CommandItemViewModel> OnDeleteRequested;

        public CommandItemViewModel(CommandItem item, CommandExecutor executor)
        {
            Item = item;
            _executor = executor;

            ExecuteCommand = new RelayCommand(_ => ExecuteItem());
            EditCommand = new RelayCommand(_ => OnEditRequested?.Invoke(this));
            DeleteCommand = new RelayCommand(_ => OnDeleteRequested?.Invoke(this));
        }

        private void ExecuteItem()
        {
            bool started = _executor.Execute(Item);

            LastExecutionSuccess = started;
            HasRecentExecution = true;
            ScheduleHighlightReset();
        }

        private async void ScheduleHighlightReset()
        {
            _highlightResetCts?.Cancel();
            _highlightResetCts = new CancellationTokenSource();
            var token = _highlightResetCts.Token;

            try
            {
                await Task.Delay(2000, token);

                if (!token.IsCancellationRequested)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HasRecentExecution = false;
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation because a newer command execution occurred.
            }
        }
    }
}