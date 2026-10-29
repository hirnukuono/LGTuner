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
                foreach (var prefabSpawner2 in assetRef.AssetAs<GameObject>().GetComponentsInChildren<LG_PrefabSpawner>())
                {
                    if (prefabSpawner2.transform.name.Contains("BuildingPart_StorageDivider_Conc_4x4x1_a", StringComparison.OrdinalIgnoreCase))
                        prefabSpawner2.transform.localPosition -= new Vector3(0f, 0.125f, 0f);                    
                }
                Logger.Debug("navmesh fix on samdb_v1 'Mining_exit_V1' done");
            }

            _geoFixesApplied = true;
        }
    }
}
