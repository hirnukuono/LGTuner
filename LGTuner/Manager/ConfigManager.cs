using AssetShards;
using BepInEx;
using GTFO.API.Utilities;
using HarmonyLib;
using LGTuner.Configs;
using LGTuner.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LGTuner.Manager
{
    public static class ConfigManager
    {
        public static IEnumerable<LayoutConfig> Layouts => _layouts;
        private static readonly List<LayoutConfig> _layouts = new();
        private static readonly Dictionary<uint, LayoutConfig> _lookup = new();
        private static readonly Dictionary<string, LayoutConfig> _fileNameLookup = new();

        public static void RenameFiles()
        {
            string assetBundleDir = Path.Combine(Paths.BepInExRootPath, "Assets", "AssetBundles");
            string wrongbundleDir = Path.Combine(Paths.ConfigPath, "Assets", "AssetBundles");
            string[] bundlePaths = Directory.GetFiles(assetBundleDir, "*", SearchOption.AllDirectories).ToArray();
            if (Directory.Exists(wrongbundleDir)) foreach (var p in Directory.GetFiles(wrongbundleDir, "'", SearchOption.AllDirectories).ToArray()) bundlePaths.AddItem<string>(p);
            foreach (var gaa in bundlePaths)
            {
                var te = gaa.Split("\\").ToArray().Last();
                if (!EntryPoint._bundlesToRename.Value.Contains(te)) continue;
                try
                {
                    Logger.Info($"renaming file {te} to {te}.fdfd.manifest");
                    File.Move(gaa, gaa + ".fdfd.manifest");
                }
                catch { }
            }


        }

        public static void LoadShardForFixingAssets()
        {
            AssetShardManager.LoadShard(AssetShardManager.GetShardName(AssetBundleName.Complex_Service, AssetBundleShard.S6));
            AssetShardManager.LoadShard(AssetShardManager.GetShardName(AssetBundleName.Complex_Tech, AssetBundleShard.S10));

            string assetBundleDir = Path.Combine(Paths.BepInExRootPath, "Assets", "AssetBundles");
            string[] bundlePaths = Directory.GetFiles(assetBundleDir, "*", SearchOption.AllDirectories).ToArray();
            foreach (var f in bundlePaths)
            {

                if (!f.Contains(".fdfd.manifest")) continue;
                Logger.Info($"loading assetbundle {f}");
                var te = f.Split("\\").ToArray().Last().Split(".").ToArray().First();
                UnityEngine.AssetBundle t = UnityEngine.AssetBundle.LoadFromFile(f);
                if (EntryPoint._bundlesToLoadAllFrom.Value.Contains(te))
                {
                    Logger.Info($"bundle {t.name} file {te} marked as load-all-prefabs");
                    EntryPoint.BundleLoadAllLookup.Add(t);
                }
                foreach (var c in t.GetAllAssetNames()) EntryPoint.BundleLookup.Add(c.ToUpperInvariant(), t);
            }
        }

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
            liveEdit.FileCreated += LiveEdit_FileChanged;
            liveEdit.StartListen();
        }

        private static void LiveEdit_FileChanged(LiveEditEventArgs e)
        {
            var key = Path.GetFileName(e.FullPath).ToLowerInvariant();
            var extension = Path.GetExtension(e.FullPath);
            if (extension.Equals(".json", StringComparison.InvariantCulture) ||
                extension.Equals(".jsonc", StringComparison.InvariantCulture))
            {
                Logger.Error($"File Edited: '{key}' '{e.FullPath}'");

                try
                {
                    uint oldID = 0;
                    bool newfile = false;
                    LayoutConfig data = null;
                    if (_fileNameLookup.ContainsKey(key)) data = _fileNameLookup[key];
                    if (data == null) newfile = true;
                    if (data != null) oldID = data.LevelLayoutID;

                    LiveEdit.TryReadFileContent(e.FullPath, (json) =>
                    {
                        try
                        {
                            var newData = JSON.Deserialize<LayoutConfig>(json);
                            if (oldID > 0) newData.LevelLayoutID = oldID;
                            if (oldID == 0) oldID = newData.LevelLayoutID;

                            if (newfile)
                            {
                                if (_fileNameLookup.ContainsKey(key)) _fileNameLookup.Remove(key);
                                //_fileNameLookup.Add(key, newData);
                                LayoutConfig toremove = null;
                                foreach (var l in _layouts) if (l.LevelLayoutID == newData.LevelLayoutID) toremove = l;
                                if (toremove != null) _layouts.Remove(toremove);
                                if (_lookup.ContainsKey(oldID)) _lookup.Remove(oldID);
                            }

                            if (!newfile)
                            {
                                _fileNameLookup.Remove(key);
                                _lookup.Remove(oldID);
                                _layouts.Remove(data);
                            }

                            _layouts.Add(newData);
                            _lookup.Add(oldID, newData);
                            _fileNameLookup.Add(key, newData);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Error while reading or inserting LGTuner Config: {ex}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error while reading LGTuner Config: {ex}");
                }
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
