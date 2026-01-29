using AssetShards;
using BepInEx;
using FluffyUnderware.DevTools.Extensions;
using GameData;
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


        public static void LoadData()
        {
            /// find out if complexresourceset and marker datablocks contain funny bundle assets
            List<string> prefabstoload = new();
            List<uint> crses = new();
            crses.Add(RundownManager.ActiveExpedition.Expedition.ComplexResourceData);

            foreach (var b in MiningMarkerDataBlock.GetAllBlocks())
                if (b != null)
                    if (b.CommonData != null)
                        foreach (var c in b.CommonData.Compositions)
                            if (c != null)
                                if (c.prefab != null)
                                    if (EntryPoint.BundleLookup.ContainsKey(c.prefab.ToUpperInvariant()) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(c.prefab.ToUpperInvariant()))
                                    {
                                        UnityEngine.Object asset = EntryPoint.BundleLookup[c.prefab.ToUpperInvariant()].LoadAsset(c.prefab.ToUpperInvariant());
                                        try { AssetShardManager.s_loadedAssetsLookup.Add(c.prefab.ToUpperInvariant(), asset); } catch { }
                                        EntryPoint.CustomMarkerPrefabs.Add(c.prefab.ToUpperInvariant());
                                    }

            foreach (var b in TechMarkerDataBlock.GetAllBlocks())
                if (b != null)
                    if (b.CommonData != null)
                        foreach (var c in b.CommonData.Compositions)
                            if (c != null)
                                if (c.prefab != null)
                                    if (EntryPoint.BundleLookup.ContainsKey(c.prefab.ToUpperInvariant()) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(c.prefab.ToUpperInvariant()))
                                    {
                                        UnityEngine.Object asset = EntryPoint.BundleLookup[c.prefab.ToUpperInvariant()].LoadAsset(c.prefab.ToUpperInvariant());
                                        try { AssetShardManager.s_loadedAssetsLookup.Add(c.prefab.ToUpperInvariant(), asset); } catch { }
                                        EntryPoint.CustomMarkerPrefabs.Add(c.prefab.ToUpperInvariant());

                                    }

            foreach (var b in ServiceMarkerDataBlock.GetAllBlocks())
                if (b != null)
                    if (b.CommonData != null)
                        foreach (var c in b.CommonData.Compositions)
                            if (c != null)
                                if (c.prefab != null)
                                    if (EntryPoint.BundleLookup.ContainsKey(c.prefab.ToUpperInvariant()) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(c.prefab.ToUpperInvariant()))
                                    {
                                        UnityEngine.Object asset = EntryPoint.BundleLookup[c.prefab.ToUpperInvariant()].LoadAsset(c.prefab.ToUpperInvariant());
                                        try { AssetShardManager.s_loadedAssetsLookup.Add(c.prefab.ToUpperInvariant(), asset); } catch { }
                                        EntryPoint.CustomMarkerPrefabs.Add(c.prefab.ToUpperInvariant());
                                    }

            foreach (var di in RundownManager.ActiveExpedition.DimensionDatas)
            {
                if (!crses.Contains(DimensionDataBlock.GetBlock(di.DimensionData).DimensionData.DimensionResourceSetID))
                    crses.Add(DimensionDataBlock.GetBlock(di.DimensionData).DimensionData.DimensionResourceSetID);
            }

            foreach (var crs in crses)
            {
                var tempdb = ComplexResourceSetDataBlock.GetBlock(crs);
                foreach (var g in tempdb.CustomGeomorphs_Challenge_1x1)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.CustomGeomorphs_Exit_1x1)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.CustomGeomorphs_Objective_1x1)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.GeomorphTiles_1x1)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.GeomorphTiles_2x1)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.GeomorphTiles_2x2)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.ElevatorShafts_1x1)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.PlugCaps)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.DoubleDropPlugsNoGates)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.SingleDropPlugsNoGates)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.StraightPlugsNoGates)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.DoubleDropPlugsWithGates)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.SingleDropPlugsWithGates)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
                foreach (var g in tempdb.StraightPlugsWithGates)
                    if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            }

            foreach (var g in prefabstoload)
                if (EntryPoint.BundleLookup.ContainsKey(g) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(g))
                    foreach (var temp in EntryPoint.BundleLookup)
                    {
                        if (temp.Key == g && !EntryPoint.BundleLoadAllLookup.Contains(temp.Value))
                        {
                            Logger.Info($"loading bundle asset prefab {g} ..");
                            UnityEngine.Object asset = temp.Value.LoadAsset(g);
                            try { AssetShardManager.s_loadedAssetsLookup.Add(g.ToUpperInvariant(), asset); } catch { }
                        }
                        if (temp.Key == g && EntryPoint.BundleLoadAllLookup.Contains(temp.Value))
                        {
                            Logger.Info($"loading all assets from bundle {temp.Value.name}");
                            foreach (var a in temp.Value.GetAllAssetNames())
                            {
                                UnityEngine.Object asset = temp.Value.LoadAsset(a);
                                try { AssetShardManager.s_loadedAssetsLookup.Add(a.ToUpperInvariant(), asset); } catch { }
                            }
                        }
                    }
        }


        public static void UnloadData()
        {
            List<UnityEngine.Object> listToRemove = new();
            foreach (var b in EntryPoint.BundleLookup)
            {
                if (AssetShardManager.s_loadedAssetsLookup.ContainsKey(b.Key.ToUpperInvariant()) && !EntryPoint.CustomMarkerPrefabs.Contains(b.Key))
                {
                    listToRemove.Add(AssetShardManager.s_loadedAssetsLookup[b.Key.ToUpperInvariant()]);
                    AssetShardManager.s_loadedAssetsLookup.Remove(b.Key.ToUpperInvariant());
                }
            }
            foreach (var a in listToRemove) UnityEngine.Object.Destroy(a); 
        }

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
                    File.Move(gaa, gaa + ".fdfd.manifest", true);
                }
                catch { }
            }
        }

        public static void LoadCustomBundles()
        {

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
