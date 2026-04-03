using MaterialDesignThemes.Wpf;
using ScriptLauncher.Models;
using ScriptLauncher.Services;
using System;
using System.Windows.Input;

namespace ScriptLauncher.ViewModels
{
    public class CommandItemViewModel
    {
        private readonly CommandExecutor _executor;
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

        public event Action<CommandItemViewModel> OnEditRequested;
        public event Action<CommandItemViewModel> OnDeleteRequested;

        public CommandItemViewModel(CommandItem item, CommandExecutor executor)
        {
            Item = item;
            _executor = executor;

            ExecuteCommand = new RelayCommand(_ => _executor.Execute(Item));
            EditCommand = new RelayCommand(_ => OnEditRequested?.Invoke(this));
            DeleteCommand = new RelayCommand(_ => OnDeleteRequested?.Invoke(this));
        }
    }
}