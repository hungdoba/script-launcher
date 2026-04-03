using ScriptLauncher.Models;
using ScriptLauncher.Services;
using ScriptLauncher.ViewModels;
using System.Windows;

namespace ScriptLauncher.Views
{
    public partial class CommandEditWindow : Window
    {
        private readonly CommandExecutor _executor = new CommandExecutor();

        public CommandItem Result { get; private set; }

        public CommandEditWindow(CommandItemViewModel existing = null)
        {
            InitializeComponent();

            var vm = new CommandEditViewModel(existing);

            // ViewModel tells the View to close — View's only job
            vm.SaveRequested += item => { Result = item; DialogResult = true; };
            vm.TestRequested += item =>
            {
                bool started = _executor.Execute(item);
                vm.SetTestStatus(started
                    ? "Test command started."
                    : "Test command failed to start.");
            };
            vm.CancelRequested += () => { DialogResult = false; };

            DataContext = vm;
        }
    }
}