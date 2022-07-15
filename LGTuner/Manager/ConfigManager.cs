using GameData;
using GTFO.API.Utilities;
using LevelGeneration;
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

            //LG_Factory.add_OnFactoryBuildDone(new Action(DumpLevel));

            var liveEdit = LiveEdit.CreateListener(configPath, "*.*", true);
            liveEdit.FileChanged += LiveEdit_FileChanged;
            liveEdit.StartListen();
        }

        private static void LiveEdit_FileChanged(LiveEditEventArgs e)
        {
            var key = Path.GetFileName(e.FullPath).ToLowerInvariant();
            Logger.Error($"File Edited: '{key}' '{e.FullPath}'");

            try
            {
                var data = _fileNameLookup[key];
                var oldID = data.LevelLayoutID;

                var json = File.ReadAllText(e.FullPath);
                var newData = JSON.Deserialize<LayoutConfig>(json);
                newData.LevelLayoutID = oldID;

                _fileNameLookup.Remove(key);
                _lookup.Remove(oldID);
                _layouts.Remove(data);

                _layouts.Add(newData);
                _lookup.Add(oldID, newData);
                _fileNameLookup.Add(key, newData);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while reading LGTuner Config: {ex}");
            }
        }

        private static void DumpLevel()
        {
            
        }

        private static void DumpLayerToConfig()
        {

        }

        public static bool TryGetConfig(uint layoutID, out LayoutConfig config)
        {
            return _lookup.TryGetValue(layoutID, out config);
        }
    }
}
