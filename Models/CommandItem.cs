namespace ScriptLauncher.Models
{
    public class CommandItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ScriptType Type { get; set; } = ScriptType.Cmd;
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool RunAsAdministrator { get; set; }
        public bool OpenWindow { get; set; } = false;
        public string Icon { get; set; }
    }
}
