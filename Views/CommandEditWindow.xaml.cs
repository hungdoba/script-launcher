using ScriptLauncher.Models;
using ScriptLauncher.ViewModels;
using System.Windows;

namespace ScriptLauncher.Views
{
    public partial class CommandEditWindow : Window
    {
        public CommandItem Result { get; private set; }

        public CommandEditWindow(CommandItemViewModel existing = null)
        {
            InitializeComponent();

            var vm = new CommandEditViewModel(existing);

            // ViewModel tells the View to close — View's only job
            vm.SaveRequested += item => { Result = item; DialogResult = true; };
            vm.CancelRequested += () => { DialogResult = false; };

            DataContext = vm;
        }
    }
}