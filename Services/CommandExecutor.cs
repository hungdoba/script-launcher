using ScriptLauncher.Models;
using System.Diagnostics;
using System.IO;

namespace ScriptLauncher.Services
{
    public class CommandExecutor
    {
        public void Execute(CommandItem item)
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
                        ? $"/k \"{item.Command}\" {item.Arguments}"
                        : $"/c \"{item.Command}\" {item.Arguments}";
                    break;

                case ScriptType.PowerShell:
                    psi.FileName = "powershell.exe";
                    psi.Arguments =
                        "-NoExit -ExecutionPolicy Bypass -NoProfile -File \"" +
                        Path.GetFullPath(item.Command) + "\" " +
                        item.Arguments;
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

        private static string BuildCommand(CommandItem item)
        {
            return string.IsNullOrWhiteSpace(item.Arguments)
                ? item.Command
                : $"{item.Command} {item.Arguments}";
        }
    }
}