using GameData;
using LGTuner.Configs;
using LGTuner.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LGTuner.Manager
{
    public static class ConfigManager
    {
        public static IEnumerable<LayoutConfig> Layouts => _layouts;
        private static readonly List<LayoutConfig> _layouts = new();
        private static readonly Dictionary<uint, LayoutConfig> _lookup = new();
        private static readonly Dictionary<string, LayoutConfig> _fileNameLookup = new();

        public static void Init()
        {
            if (!MTFOUtil.HasCustomContent)
                return;

            var configPath = Path.Combine(MTFOUtil.CustomPath, "LGTuner");
            var dir = Directory.CreateDirectory(configPath);

            foreach (var fileInfo in dir.GetFiles())
            {
                var extension = fileInfo.Extension;
                var isJson = extension.Equals(".json", StringComparison.InvariantCultureIgnoreCase);
                var isJsonc = extension.Equals(".jsonc", StringComparison.InvariantCultureIgnoreCase);

                if (isJson || isJsonc)
                {
                    var json = File.ReadAllText(fileInfo.FullName);
                    var data = JSON.Deserialize<LayoutConfig>(json);
                    if (_lookup.ContainsKey(data.LevelLayoutID))
                    {
                        Logger.Error($"Duplicated ID found!: {fileInfo.Name}, {data.LevelLayoutID}");
                        continue;
                    }

                    _layouts.Add(data);
                    _lookup.Add(data.LevelLayoutID, data);
                    _fileNameLookup.Add(fileInfo.Name.ToLowerInvariant(), data);
                }
            }

            var watcher = new FileSystemWatcher
            {
                Path = configPath,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.*"
            };
            watcher.Changed += new FileSystemEventHandler(OnConfigFileEdited_ReloadConfig);
            watcher.EnableRaisingEvents = true;
        }

        private static void OnConfigFileEdited_ReloadConfig(object sender, FileSystemEventArgs e)
        {
            var key = Path.GetFileName(e.Name).ToLowerInvariant();
            Logger.Error($"File Edited: '{key}' '{e.Name}'");
            if (_fileNameLookup.TryGetValue(key, out var data))
            {
                var oldID = data.LevelLayoutID;
                
                _fileNameLookup.Remove(key);
                _lookup.Remove(oldID);
                _layouts.Remove(data);

                var json = File.ReadAllText(e.Name);
                var newData = JSON.Deserialize<LayoutConfig>(json);
                newData.LevelLayoutID = oldID;

                _layouts.Add(newData);
                _lookup.Add(oldID, newData);
                _fileNameLookup.Add(key, newData);
            }
        }

        public static bool TryGetConfig(uint layoutID, out LayoutConfig config)
        {
            return _lookup.TryGetValue(layoutID, out config);
        }
    }
}
