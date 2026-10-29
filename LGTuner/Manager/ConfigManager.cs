using AmorLib.Dependencies;
using AmorLib.Utils.Extensions;
using BepInEx;
using GTFO.API.Utilities;
using LGTuner.Configs;
using LGTuner.Utils;
using System.Diagnostics.CodeAnalysis;

namespace LGTuner.Manager
{
    public static partial class ConfigManager
    {
        private static readonly Dictionary<string, HashSet<uint>> _lookups = new();
        private static readonly Dictionary<uint, LayoutConfig> _layouts = new();

        internal static void Init()
        {
            /* --- remove legacy bundle file extension --- */
            IEnumerable<string> bundlePaths = new string[]
            {
                Path.Combine(Paths.BepInExRootPath, "Assets", "AssetBundles"),
                Path.Combine(Paths.ConfigPath, "Assets", "AssetBundles")
            }.Where(Directory.Exists).SelectMany(dir => Directory.GetFiles(dir, "*", SearchOption.AllDirectories));

            string suffix = ".fdfd.manifest";
            foreach (string path in bundlePaths) 
            {
                if (!path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string newPath = path[..^suffix.Length];
                if (string.IsNullOrEmpty(Path.GetFileName(newPath)) || File.Exists(newPath))
                    continue;

                try
                {
                    Logger.Message($"Renaming file to '{newPath}'");
                    File.Move(path, newPath);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to rename '{path}' to '{newPath}':\n{ex.Message}");
                }
            }

            /* --- load mtfo custom data --- */
            if (!MTFO_Wrapper.IsLoaded || !MTFO_Wrapper.HasCustomContent)
                return;

            string modulePath = Path.Combine(MTFO_Wrapper.CustomPath, EntryPoint.MODNAME);
            string[] ext = new[] { "*.json", "*.jsonc" };
            foreach (string customFile in Directory.EnumerateFiles(modulePath, "*", SearchOption.AllDirectories).Where(f => ext.Contains("*" + Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)))
            {
                try
                {
                    string content = File.ReadAllText(customFile);
                    ReadFileContent(customFile, content);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error reading file {customFile}:\n{ex.Message}");
                }
            }

            var sfsw = SafeFileSystemWatcher.Create(modulePath, ext, true);
            sfsw.OnCreated += FileCreatedOrChanged;
            sfsw.OnChanged += FileCreatedOrChanged;
            sfsw.OnDeleted += FileDeleted;
            sfsw.OnRenamed += FileRenamed;
        }

        private static void ReadFileContent(string file, string content) 
        {
            var layoutSet = _lookups.GetOrAddNew(file);
            foreach (uint id in layoutSet)
            {
                _layouts.Remove(id);
            }
            layoutSet.Clear();

            var datas = JSON.Deserialize<List<LayoutConfig>>(content) ?? new();
            foreach (var data in datas.Where(d => d != null && d.LevelLayoutID != 0u))
            {
                layoutSet.Add(data.LevelLayoutID);
                _layouts[data.LevelLayoutID] = data;
            }
        }

        private static void FileCreatedOrChanged(FileEventArgs e)
        {
            Logger.Warn($"LiveEdit file updated: {e.FullPath}");
            ReadFileContent(e.FullPath, e.ReadContent());
        }

        private static void FileDeleted(FileEventArgs e)
        {
            Logger.Warn($"LiveEdit file deleted: {e.FullPath}");
            foreach (uint id in _lookups[e.FullPath])
            {
                _layouts.Remove(id);
            }
            _lookups.Remove(e.FullPath);
        }

        private static void FileRenamed(FileRenamedEventArgs e)
        {
           (string oldPath, string path) = (e.OldFullPath, e.FullPath);
            Logger.Warn($"LiveEdit file renamed: {path}");
            if (_lookups.ContainsKey(oldPath))
            {
                _lookups[path] = _lookups[oldPath];
                _lookups.Remove(oldPath);
            }
        }

        public static bool TryGetConfig(uint layoutID, [MaybeNullWhen(false)] out LayoutConfig config)
        {
            return _layouts.TryGetValue(layoutID, out config);
        }
    }
}