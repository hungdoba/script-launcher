using ScriptLauncher.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ScriptLauncher.Services
{
    public class CommandExecutor
    {
        public bool Execute(CommandItem item)
        {
            if (item == null)
            {
                MessageBox.Show("No command was selected.", "Script Launcher", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(item.Command) && item.Type != ScriptType.Cmd)
            {
                MessageBox.Show("This command does not have a launch target.", "Script Launcher", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                var psi = new ProcessStartInfo();

                switch (item.Type)
                {
                    case ScriptType.Cmd:
                        psi.FileName = "cmd.exe";

                        psi.Arguments = item.OpenWindow
                            ? "/k " + BuildCmdCommand(item)
                            : "/c " + BuildCmdCommand(item);
                        break;

                    case ScriptType.Batch:
                        psi.FileName = "cmd.exe";
                        psi.Arguments = item.OpenWindow
                            ? "/k " + WrapForCmd(item.Command, item.Arguments)
                            : "/c " + WrapForCmd(item.Command, item.Arguments);
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

                var process = Process.Start(psi);
                return process != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch '{item.Name ?? item.Command}': {ex.Message}",
                    "Script Launcher",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private static string BuildCommand(CommandItem item)
        {
            return string.IsNullOrWhiteSpace(item.Arguments)
                ? item.Command
                : $"{item.Command} {item.Arguments}";
        }

        private static string BuildCmdCommand(CommandItem item)
        {
            if (ShouldWrapForCmd(item.Command))
                return WrapForCmd(item.Command, item.Arguments);

            return BuildCommand(item);
        }

        private static bool ShouldWrapForCmd(string command)
        {
            return !string.IsNullOrWhiteSpace(command) &&
                   (command.IndexOf('\\') >= 0 ||
                    command.IndexOf('/') >= 0 ||
                    Path.HasExtension(command));
        }

        private static string WrapForCmd(string command, string arguments)
        {
            var quotedCommand = QuoteArgument(command);
            var commandLine = string.IsNullOrWhiteSpace(arguments)
                ? quotedCommand
                : quotedCommand + " " + arguments;

            return $"\"{commandLine}\"";
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