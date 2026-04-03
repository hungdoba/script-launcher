using ScriptLauncher.Models;
using ScriptLauncher.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace ScriptLauncher.ViewModels
{
    public class MainViewModel
    {
        private readonly JsonCommandLoader _loader;
        private readonly CommandExecutor _executor;

        public ObservableCollection<CommandItemViewModel> Commands { get; }
            = new ObservableCollection<CommandItemViewModel>();

        // Raised when the user wants to edit an existing item
        public event Action<CommandItemViewModel> EditRequested;

        // Raised when the user wants to open the Add window
        public event Action AddRequested;

        public ICommand OpenAddWindowCommand { get; }

        public string StatusMessage { get; }

        public MainViewModel()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string commandPath = Path.Combine(basePath, "Data", "commands.json");

            _loader = new JsonCommandLoader(commandPath);
            _executor = new CommandExecutor();

            if (!File.Exists(commandPath))
                StatusMessage = $"No commands file found at {commandPath}. Starting with an empty list.";

            try
            {
                foreach (var item in _loader.Load())
                    AddViewModel(new CommandItemViewModel(item, _executor));
            }
            catch (Exception ex)
            {
                Commands.Clear();
                StatusMessage = $"Failed to load commands: {ex.Message}";
            }

            OpenAddWindowCommand = new RelayCommand(_ => AddRequested?.Invoke());
        }

        // ── CRUD ─────────────────────────────────────────────────────────────

        public void AddItem(CommandItem item)
        {
            AddViewModel(new CommandItemViewModel(item, _executor));
            SaveCommands();
        }

        public void UpdateItem(CommandItemViewModel vm, CommandItem updated)
        {
            // Reflect changes onto the model in-place
            var item = vm.Item;
            item.Name = updated.Name;
            item.Description = updated.Description;
            item.Type = updated.Type;
            item.Command = updated.Command;
            item.Arguments = updated.Arguments;
            item.WorkingDirectory = updated.WorkingDirectory;
            item.RunAsAdministrator = updated.RunAsAdministrator;
            item.OpenWindow = updated.OpenWindow;
            item.Icon = updated.Icon;

            // Replace vm so bindings refresh
            int index = Commands.IndexOf(vm);
            Commands.RemoveAt(index);
            var newVm = new CommandItemViewModel(item, _executor);
            AddViewModel(newVm, index);

            SaveCommands();
        }

        public void DeleteItem(CommandItemViewModel vm)
        {
            Commands.Remove(vm);
            SaveCommands();
        }

        public void SaveCommands()
        {
            if (_loader == null) return;
            var items = new System.Collections.Generic.List<CommandItem>();
            foreach (var vm in Commands)
                items.Add(vm.Item);
            _loader.Save(items);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void AddViewModel(CommandItemViewModel vm, int index = -1)
        {
            vm.OnEditRequested += v => EditRequested?.Invoke(v);
            vm.OnDeleteRequested += v => DeleteItem(v);

            if (index < 0) Commands.Add(vm);
            else Commands.Insert(index, vm);
        }
    }
}