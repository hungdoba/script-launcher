using MaterialDesignThemes.Wpf;
using ScriptLauncher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ScriptLauncher.ViewModels
{
    public class CommandEditViewModel : INotifyPropertyChanged
    {
        public string WindowTitle { get; }

        public List<ScriptType> Types { get; } = new List<ScriptType>
        {
            ScriptType.Cmd,
            ScriptType.Batch,
            ScriptType.PowerShell,
            ScriptType.Executable
        };

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private ScriptType _selectedType;
        public ScriptType SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        private string _command;
        public string Command
        {
            get => _command;
            set { _command = value; OnPropertyChanged(); }
        }

        private string _arguments;
        public string Arguments
        {
            get => _arguments;
            set { _arguments = value; OnPropertyChanged(); }
        }

        private string _workingDirectory;
        public string WorkingDirectory
        {
            get => _workingDirectory;
            set { _workingDirectory = value; OnPropertyChanged(); }
        }

        private bool _runAsAdministrator;
        public bool RunAsAdministrator
        {
            get => _runAsAdministrator;
            set { _runAsAdministrator = value; OnPropertyChanged(); }
        }

        private bool _openWindow;
        public bool OpenWindow
        {
            get => _openWindow;
            set { _openWindow = value; OnPropertyChanged(); }
        }

        private string _iconText;
        public string IconText
        {
            get => _iconText;
            set
            {
                _iconText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewIconKind));
            }
        }

        public PackIconKind PreviewIconKind
        {
            get
            {
                if (Enum.TryParse<PackIconKind>(_iconText, out var kind))
                    return kind;
                return PackIconKind.Console;
            }
        }

        private string _validationMessage;
        public string ValidationMessage
        {
            get => _validationMessage;
            set { _validationMessage = value; OnPropertyChanged(); }
        }

        public event Action<CommandItem> SaveRequested;
        public event Action CancelRequested;
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public CommandEditViewModel(CommandItemViewModel existing = null)
        {
            if (existing != null)
            {
                WindowTitle = "Edit Command";
                Name = existing.Item.Name;
                Description = existing.Item.Description;
                SelectedType = existing.Item.Type;
                Command = existing.Item.Command;
                Arguments = existing.Item.Arguments;
                WorkingDirectory = existing.Item.WorkingDirectory;
                RunAsAdministrator = existing.Item.RunAsAdministrator;
                OpenWindow = existing.Item.OpenWindow;
                IconText = existing.Item.Icon;
            }
            else
            {
                WindowTitle = "Add Command";
                SelectedType = ScriptType.Cmd;
                IconText = "Console";
            }

            SaveCommand = new RelayCommand(_ => ExecuteSave());
            CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke());
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Command))
            {
                ValidationMessage = "Name and Command are required.";
                return;
            }

            ValidationMessage = null;

            SaveRequested?.Invoke(new CommandItem
            {
                Name = Name.Trim(),
                Description = Description?.Trim() ?? "",
                Type = SelectedType,
                Command = Command.Trim(),
                Arguments = Arguments?.Trim() ?? "",
                WorkingDirectory = WorkingDirectory?.Trim() ?? "",
                RunAsAdministrator = RunAsAdministrator,
                OpenWindow = OpenWindow,
                Icon = IconText?.Trim() ?? "Console"
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}