using BepInEx.Unity.IL2CPP;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LGTuner.Manager;
using UnityEngine;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_SetupFloor), nameof(LG_SetupFloor.Build))]
    internal static class Inject_SetupFloor_Late
    {
        private static bool _geoFixesApplied = false;

        static Inject_SetupFloor_Late()
        {
            LevelAPI.OnLevelCleanup += () => _geoFixesApplied = false;
        }

        [HarmonyPrepare]
        private static bool Prepare()
        {
            return !ConfigManager.BCGF_IsLoaded; // don't patch if BCGF present
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyWrapSafe]
        private static void Pre_SetupFloor_Build()
        {
            if (!ConfigManager.General.LegacyGeoFixesEnabled.Value || _geoFixesApplied)
                return;

            if (AssetAPI.TryGetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_01_R8D1.prefab", out var go1))
            {
                foreach (var transform in go1.GetComponentsInChildren<Transform>())
                {
                    if (transform.name.Equals("AIGraphSource", StringComparison.OrdinalIgnoreCase)) 
                        transform.position = new Vector3(5f, 2f, -16f);
                }
                Logger.Debug("navmesh fix on 'geo_64x64_service_floodways_hub_SF_01_R8D1' done");
            }

            if (AssetAPI.TryGetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_01.prefab", out var go2))
            {
                foreach (var transform2 in go2.GetComponentsInChildren<Transform>())
                {
                    if (transform2.name.Equals("AIGraphSource", StringComparison.OrdinalIgnoreCase))
                        transform2.position = new Vector3(5f, 2f, -16f);
                }
                Logger.Debug("navmesh fix on 'geo_64x64_service_floodways_hub_SF_01' done");
            }

            if (AssetAPI.TryGetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_HA_05.prefab", out var go3))
            {
                foreach (var prefabSpawner in go3.GetComponentsInChildren<LG_PrefabSpawner>())
                {
                    if (prefabSpawner.transform.name.Contains("prop_generic_duct_d_2m_tile_001"))
                    {
                        foreach (var transform3 in prefabSpawner.m_prefab.GetComponentsInChildren<Transform>())
                        {
                            if (transform3.name.Equals("Capsulecollider", StringComparison.OrdinalIgnoreCase))
                                transform3.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                        }
                    }
                }
                Logger.Debug("collider fix on 'geo_64x64_tech_lab_HA_05' done");
            }

            if (!IL2CPPChainloader.Instance.Plugins.ContainsKey("StairsFix") && AssetAPI.TryGetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Mining_Shaft.prefab", out var go4))
            {
                var ladders = go4.GetComponentsInChildren<LG_Ladder>();
                if (ladders.Count >= 2)
                {
                    ladders[2].transform.position += new Vector3(0f, 0f, -1.5f);
                    Logger.Debug("navmesh fix on 'Dimension_Desert_Mining_Shaft' done");
                }
            }

            if (AssetAPI.AllCustomAssets.TryGet("Assets/Custom Geo's/Mining exit/Mining_exit_V1.prefab", out var assetRef) && assetRef.IsLoaded)
            {
                var positionArr = new Vector3[] // since modded geo may not always be unloaded, we need to use absolute positions instead of relative offset
                {
                    new(-3.5f,     -4.119f, -21.95f),
                    new(-3.5f,     -4.119f, -17.05f),
                    new( 3.46f,    -4.119f, -21.95f),
                    new( 3.46f,    -4.119f, -17f),
                    new( 3.5f,     -4.119f, -21.95f),
                    new( 0.20444f, -4.119f, -23.95f),
                    new(-3.5f,     -4.119f, -11.1f),
                    new( 3.46f,    -4.119f, -11.07f),
                    new(-3.5f,     -4.119f, -5.21f),
                    new( 3.46f,    -4.119f, -5.16f),
                    new(-8.443f,   -4.119f, -27.37f),
                    new( 8.45f,    -4.119f, -27.265f),
                    new(-3.5f,     -4.119f, -21.95f),
                    new(-3.5f,     -4.119f, -17.05f),
                    new( 3.46f,    -4.119f, -21.95f),
                    new( 3.46f,    -4.119f, -17f),
                    new( 3.5f,     -4.119f, -21.95f),
                    new( 0.20444f, -4.119f, -23.95f),
                    new(-3.5f,     -4.205f, -13.11f),
                    new( 3.46f,    -4.119f, -11.07f),
                    new(-8.443f,   -4.119f, -27.37f),
                    new( 8.45f,    -4.119f, -27.265f),
                    new(-3.5f,     -4.119f, -28.07f),
                    new( 3.46f,    -4.119f, -28f)
                };
                int idx = 0;
                foreach (var prefabSpawner2 in assetRef.AssetAs<GameObject>().GetComponentsInChildren<LG_PrefabSpawner>())
                {
                    if (prefabSpawner2.transform.name.Contains("BuildingPart_StorageDivider_Conc_4x4x1_a", StringComparison.OrdinalIgnoreCase))
                        prefabSpawner2.transform.localPosition = positionArr[idx++];                
                }
                Logger.Debug("navmesh fix on samdb_v1 'Mining_exit_V1' done");
            }

            _geoFixesApplied = true;
        }
    }
}
