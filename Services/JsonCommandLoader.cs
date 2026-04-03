using ScriptLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace ScriptLauncher.Services
{
    public class ScriptTypeConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
            => new[] { typeof(ScriptType) };

        public override object Deserialize(IDictionary<string, object> dictionary,
            Type type, JavaScriptSerializer serializer)
            => throw new NotImplementedException(); // handled via string path below

        public override IDictionary<string, object> Serialize(object obj,
            JavaScriptSerializer serializer)
            => throw new NotImplementedException(); // not used for primitive enum
    }

    public class JsonCommandLoader
    {
        private readonly string _filePath;

        public JsonCommandLoader(string filePath)
        {
            _filePath = filePath;
        }

        public List<CommandItem> Load()
        {
            if (!File.Exists(_filePath))
                return new List<CommandItem>();

            var json = File.ReadAllText(_filePath);
            var serializer = new JavaScriptSerializer();
            var rawList = serializer.Deserialize<List<Dictionary<string, object>>>(json);

            var result = new List<CommandItem>();
            foreach (var dict in rawList)
            {
                var item = new CommandItem();
                if (dict.TryGetValue("name", out var v)) item.Name = v?.ToString();
                if (dict.TryGetValue("description", out v)) item.Description = v?.ToString();
                if (dict.TryGetValue("command", out v)) item.Command = v?.ToString();
                if (dict.TryGetValue("arguments", out v)) item.Arguments = v?.ToString();
                if (dict.TryGetValue("workingDirectory", out v)) item.WorkingDirectory = v?.ToString();
                if (dict.TryGetValue("icon", out v)) item.Icon = v?.ToString();
                if (dict.TryGetValue("runAsAdministrator", out v)) item.RunAsAdministrator = v is bool b && b;
                if (dict.TryGetValue("openWindow", out v)) item.OpenWindow = v is bool b2 && b2;

                if (dict.TryGetValue("type", out v) &&
                    Enum.TryParse<ScriptType>(v?.ToString(), ignoreCase: true, out var st))
                    item.Type = st;
                else
                    item.Type = ScriptType.Cmd;

                result.Add(item);
            }

            return result;
        }

        public void Save(List<CommandItem> items)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var rawList = new List<Dictionary<string, object>>();
            foreach (var item in items)
            {
                rawList.Add(new Dictionary<string, object>
                {
                    ["name"] = item.Name ?? "",
                    ["description"] = item.Description ?? "",
                    ["type"] = item.Type.ToString(),   // ✅ "Cmd" not 0
                    ["command"] = item.Command ?? "",
                    ["arguments"] = item.Arguments ?? "",
                    ["workingDirectory"] = item.WorkingDirectory ?? "",
                    ["runAsAdministrator"] = item.RunAsAdministrator,
                    ["openWindow"] = item.OpenWindow,
                    ["icon"] = item.Icon ?? "Console"
                });
            }

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(rawList);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
        }
    }
}