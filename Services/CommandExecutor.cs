using ScriptLauncher.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ScriptLauncher.Services
{
    public class CommandExecutor
    {
        public void Execute(CommandItem item)
        {
            if (item == null)
            {
                MessageBox.Show("No command was selected.", "Script Launcher", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(item.Command) && item.Type != ScriptType.Cmd)
            {
                MessageBox.Show("This command does not have a launch target.", "Script Launcher", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var psi = new ProcessStartInfo();

                switch (item.Type)
                {
                    case ScriptType.Cmd:
                        psi.FileName = "cmd.exe";

                        // ✅ Open window or silent
                        psi.Arguments = item.OpenWindow
                            ? "/k " + BuildCommand(item)
                            : "/c " + BuildCommand(item);
                        break;

                    case ScriptType.Batch:
                        psi.FileName = "cmd.exe";
                        psi.Arguments = item.OpenWindow
                            ? $"/k {QuoteArgument(item.Command)}{FormatArguments(item.Arguments)}"
                            : $"/c {QuoteArgument(item.Command)}{FormatArguments(item.Arguments)}";
                        break;

                    case ScriptType.PowerShell:
                        psi.FileName = "powershell.exe";
                        psi.Arguments = (item.OpenWindow ? "-NoExit " : string.Empty)
                            + "-ExecutionPolicy Bypass -NoProfile -File "
                            + QuoteArgument(Path.GetFullPath(item.Command))
                            + FormatArguments(item.Arguments);
                        break;

                    case ScriptType.Executable:
                        psi.FileName = item.Command;
                        psi.Arguments = item.Arguments ?? "";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(item.WorkingDirectory))
                    psi.WorkingDirectory = item.WorkingDirectory;

                // ✅ Silent vs visible
                if (!item.OpenWindow)
                {
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.UseShellExecute = false;
                }
                else
                {
                    psi.UseShellExecute = true;
                }

                // ✅ Admin
                if (item.RunAsAdministrator)
                {
                    psi.UseShellExecute = true;
                    psi.Verb = "runas";
                }

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch '{item.Name ?? item.Command}': {ex.Message}",
                    "Script Launcher",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static string BuildCommand(CommandItem item)
        {
            return string.IsNullOrWhiteSpace(item.Arguments)
                ? item.Command
                : $"{item.Command} {item.Arguments}";
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "\"\"";

            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

        private static string FormatArguments(string arguments)
        {
            return string.IsNullOrWhiteSpace(arguments)
                ? string.Empty
                : " " + arguments;
        }
    }
}