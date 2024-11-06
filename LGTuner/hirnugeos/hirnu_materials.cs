using UnityEngine;
using LevelGeneration;
using Expedition;
using GTFO.API;
using System.Collections.Generic;

namespace LGTuner.hirnugeos
{
    public class Hirnu_materials : MonoBehaviour
    {
        public static void OnFactoryBuildDone()
        {
            EntryPoint.hirnufloormaterial = null;
            EntryPoint.hirnuwallmaterial = null;

            List<GameObject> tmpset = new();
            tmpset.Clear();

            SubComplex subcomplex = Builder.LayerBuildDatas[0].m_zoneBuildDatas[0].SubComplex;
            Debug.Log($"hirnugeos - building materials for {subcomplex}");

            if (subcomplex == SubComplex.DigSite || subcomplex == SubComplex.Refinery)
            {
                var geo = AssetAPI.GetLoadedAsset("Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_32x32_elevator_shaft_dig_site_03.prefab").Cast<GameObject>();
                Debug.Log("hirnugeos - materials digsite going");
                foreach (var mtr in geo.GetComponentsInChildren<LG_PrefabSpawner>(true))
                {
                    if (EntryPoint.hirnufloormaterial != null && EntryPoint.hirnuwallmaterial != null) Debug.Log("hirnugeos - materials breaking");
                    if (EntryPoint.hirnufloormaterial != null && EntryPoint.hirnuwallmaterial != null) break;
                    if (mtr.m_prefab.name.Contains("BuildingPart_StorageGround_Gravel_2x2_a"))
                    {
                        foreach (var tmpgo in mtr.m_prefab.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (tmpgo.gameObject.name.Contains("g_BuildingPart"))
                            {
                                EntryPoint.hirnufloormaterial = Object.Instantiate(tmpgo.material);
                                EntryPoint.hirnuwallmaterial = Object.Instantiate(tmpgo.material);
                            }
                        }
                    }
                }
            }
            Debug.Log("hirnugeos - materials digsite done");

            if (subcomplex == SubComplex.Lab || subcomplex == SubComplex.DataCenter)
            {
                var geo = AssetAPI.GetLoadedAsset("Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_HSU.prefab").Cast<GameObject>();
                foreach (var mtr in geo.GetComponentsInChildren<LG_PrefabSpawner>(true))
                {
                    if (EntryPoint.hirnufloormaterial != null) break;
                    if (mtr.m_prefab.name.Contains("BuildingPart_Lab_Floor_4x4_a"))
                    {
                        foreach (var tmpgo in mtr.m_prefab.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (tmpgo.gameObject.name.Contains("g_BuildingPart"))
                            {
                                EntryPoint.hirnufloormaterial = Object.Instantiate(tmpgo.material);
                            }
                        }
                    }
                }
                var geo2 = AssetAPI.GetLoadedAsset("Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_07_LF.prefab").Cast<GameObject>();
                foreach (var mtr2 in geo2.GetComponentsInChildren<LG_PrefabSpawner>(true))
                {
                    if (EntryPoint.hirnufloormaterial != null && EntryPoint.hirnuwallmaterial != null) Debug.Log("hirnugeos - materials breaking");
                    if (EntryPoint.hirnufloormaterial != null && EntryPoint.hirnuwallmaterial != null) break;
                    if (mtr2.m_prefab.name.Contains("BuildingPart_DataCenter_6x6_Roof_c"))
                    {
                        foreach (var tmpgo in mtr2.m_prefab.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (tmpgo.gameObject.name.Contains("g_BuildingPart"))
                            {
                                EntryPoint.hirnuwallmaterial = Object.Instantiate(tmpgo.material);
                            }
                        }
                    }
                }
            }

            if (subcomplex == SubComplex.Floodways || subcomplex == SubComplex.Gardens)
            {
                var geo = AssetAPI.GetLoadedAsset("Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_AW_01.prefab").Cast<GameObject>();
                foreach (var mtr in geo.GetComponentsInChildren<LG_PrefabSpawner>(true))
                {
                    if (EntryPoint.hirnufloormaterial != null && EntryPoint.hirnuwallmaterial != null) Debug.Log("hirnugeos - materials breaking");
                    if (EntryPoint.hirnufloormaterial != null && EntryPoint.hirnuwallmaterial != null) break;
                    if (mtr.m_prefab.name.Contains("edgeup"))
                    {
                        foreach (var tmpgo in mtr.m_prefab.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (tmpgo.gameObject.name.Contains("g_Service"))
                            {
                                EntryPoint.hirnufloormaterial = Object.Instantiate(tmpgo.material);
                                EntryPoint.hirnuwallmaterial = Object.Instantiate(tmpgo.material);
                            }
                        }
                    }
                }
            }
            Debug.Log($"hirnugeos - materials done {EntryPoint.hirnufloormaterial == null} {EntryPoint.hirnuwallmaterial == null}");

            foreach (var geo in tmpset) geo.SetActive(false);

            foreach (var go in FindObjectsOfType<LG_Geomorph>())
            {
                foreach (var mr in go.GetComponentsInChildren<MeshRenderer>())
                {
                    if (mr.gameObject.name.Contains("hirnu_floor")) mr.SetMaterial(EntryPoint.hirnufloormaterial);
                    if (mr.gameObject.name.Contains("hirnu_wall")) mr.SetMaterial(EntryPoint.hirnuwallmaterial);
                }
            }
        }

        public static void OnFactoryBuildStart()
        {
            if (EntryPoint.hirnulamp == null)
            {
                GameObject tmpset = AssetAPI.GetLoadedAsset("Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_32x32_elevator_shaft_lab_03.prefab").Cast<GameObject>();
                foreach (var asd in tmpset.GetComponentsInChildren<LG_PrefabSpawner>())
                {
                    if (asd.m_prefab.name == "prop_generic_light_regular_wall_fluorescent_02") { EntryPoint.hirnulamp = Instantiate(asd); EntryPoint.hirnulampfixture = Instantiate(EntryPoint.hirnulamp.m_prefab); }
                    if (EntryPoint.hirnulamp != null) break;
                }
                //tmpset.Destroy();
                EntryPoint.hirnulamp.m_prefab = EntryPoint.hirnulampfixture;
                EntryPoint.hirnulamp.transform.position = new(0, -50, 0);
                EntryPoint.hirnulampfixture.transform.position = new(0, -50, 0);
            }
        }
    }
}