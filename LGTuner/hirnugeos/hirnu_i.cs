﻿using UnityEngine;
using AIGraph;
using LevelGeneration;
using Expedition;
using GTFO.API;
using GameData;
using FluffyUnderware.DevTools.Extensions;

namespace LGTuner.hirnugeos
{
    public class Hirnu_i : MonoBehaviour
    {

        public static void OnFactoryBuildDone()
        {
            if (EntryPoint.hirnuigo != null) EntryPoint.hirnuigo.SetActive(false);
        }
        public static void OnFactoryBuildStart(SubComplex subcomplex, LG_Zone zone)
        {
            //mining floor + wall "BuildingPart_StorageGround_Gravel_2x2_a"
            // tech floor        "BuildingPart_Lab_Floor_4x4_a"
            //tech wall         "BuildingPart_Lab_Wall_4x8_solid_a"
            //service floor +wall "ServiceCorridor_Ground_6x6_edgeup"

            EntryPoint.hirnuigo = null;
            EntryPoint.hirnuigo = new();
            Debug.Log("LGTuner - hirnugeos - building i-tile");
            EntryPoint.hirnuigo.SetActive(true);
            EntryPoint.hirnuigo.hideFlags = HideFlags.HideAndDontSave;
            EntryPoint.hirnuigo.layer = LayerManager.LAYER_DEFAULT;
            EntryPoint.hirnuigo.name = "hirnu_tile_i";
            var geo = EntryPoint.hirnuigo.AddComponent<LG_Geomorph>();
            var vnv = EntryPoint.hirnuigo.AddComponent<AIG_VoxelNodeVolume>();

            GameObject area = new("Area A");
            area.transform.parent = EntryPoint.hirnuigo.transform;
            area.name = "hirnu_tile_i_a";
            GameObject graphsource = new("AreaAIGraphSource");
            graphsource.transform.parent = area.transform;
            graphsource.transform.localPosition += new Vector3(0, 2f, 0);
            var tmp2 = graphsource.AddComponent<LG_AreaAIGraphSource>();
            tmp2.m_position = new(0, 2f, 0);
            var area2 = area.AddComponent<LG_Area>();
            area2.m_size = LG_AreaSize.Medium;
            area2.m_geomorph = geo;
            geo.m_areas = new[] { area2 };
            GameObject bg = new("Gate 1");
            bg.transform.parent = area.transform;
            bg.transform.localPosition = new Vector3(0, 0, 32);
            bg.transform.localEulerAngles = new(0, 0, 0);
            var bgg = bg.AddComponent<LG_Plug>();
            bgg.m_originalForward = new(0, 0, 1);
            bgg.m_hasOriginalForward = true;
            bgg.m_subComplex = subcomplex;
            GameObject sg = new("Gate 2");
            sg.transform.parent = area.transform;
            sg.transform.localPosition = new Vector3(0, 0, -32);
            sg.transform.localEulerAngles = new(0, 180, 0);
            var sgg = sg.AddComponent<LG_Plug>();
            sgg.m_originalForward = new(0, 0, -1);
            sgg.m_hasOriginalForward = true;
            sgg.m_subComplex = subcomplex;
            area2.m_groupSource = tmp2;
            area2.m_geomorph = geo;

            GameObject props = new("EnvProps");
            props.transform.parent = area.transform;
            props.transform.localPosition = new(0, 0, 0);

            // floor
            for (int i = -30; i < 32; i = i + 4) for (int j = -30; j < 32; j = j + 4)
                {
                    if (i < -24 && j > -8 && j < 8) continue;
                    if (i > 24 && j > -8 && j < 8) continue;
                    if (j < -24 && i > -8 && i < 8) continue;
                    if (j > 24 && i > -8 && i < 8) continue;
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    floor.transform.name = "hirnu_floor";
                    floor.transform.parent = props.transform;
                    floor.transform.localPosition = new(i, -0.05f, j);
                    floor.transform.localEulerAngles = new(90, 0, 0); // 90 90 0
                    floor.transform.localScale = new Vector3(4, 4, 4);
                }

            // ceiling
            if (!zone.Dimension.DimensionData.IsOutside)
            {
                GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Quad);
                ceiling.transform.parent = props.transform;
                ceiling.transform.localPosition = new(0, 8, 0);
                ceiling.transform.localEulerAngles = new(270, 0, 0);
                ceiling.transform.localScale = new Vector3(64, 64, 64);
                ceiling.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Texture2D.blackTexture);
            }

            // lights

            GameObject lights = new("Lights");
            lights.transform.parent = area.transform;
            lights.transform.localPosition = new(0, 0, 0);

            for (int i = -24; i < 25; i = i + 16)
            {
                GameObject tmplamppu = Instantiate(EntryPoint.hirnulamp.gameObject);
                GameObject l1 = new($"Light {i} west");
                l1.transform.parent = lights.transform;
                l1.transform.localPosition = new(-7.9f, 7, i);
                l1.transform.localEulerAngles = new(300, 270, 0);
                tmplamppu.transform.parent = l1.transform;
                tmplamppu.transform.localPosition = new(0, 0, 0);
                tmplamppu.transform.localEulerAngles = new(0, 0, 0);

                GameObject tmplamppu2 = Instantiate(EntryPoint.hirnulamp.gameObject);
                GameObject l2 = new($"Light {i} east");
                l2.transform.parent = lights.transform;
                l2.transform.localPosition = new(7.9f, 7, i);
                l2.transform.localEulerAngles = new(300, 90, 0);
                tmplamppu2.transform.parent = l2.transform;
                tmplamppu2.transform.localPosition = new(0, 0, 0);
                tmplamppu2.transform.localEulerAngles = new(0, 0, 0);
            }

            // walls
            GameObject wallw = new GameObject("fd_wall_w");
            GameObject walle = new GameObject("fd_wall_e");
            wallw.name = "fd_wall_w";
            wallw.transform.SetParent(props.transform);
            wallw.transform.localPosition = new(-8, 4, 0);
            wallw.transform.localEulerAngles = new(0, 270, 0);

            for (int i = -30; i < 33; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = wallw.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            walle.name = "fd_wall_e";
            walle.transform.SetParent(props.transform);
            walle.transform.localPosition = new(8, 4, 0);
            walle.transform.localEulerAngles = new(0, 90, 0);
            for (int i = -30; i < 33; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = walle.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            // markers
            MarkerGroupDataBlock mgroup = MarkerGroupDataBlock.GetBlock("Tech_DataCenter");
            GameObject markers = new("Markers");
            markers.transform.parent = area.transform;

            // fallback
            GameObject fb1 = new("Fallback 1");
            fb1.transform.parent = markers.transform;
            var fbm1 = fb1.AddComponent<TechDataCenterMarkerProducer>();
            fbm1.m_markerDataBlockID = 63;

            // signs
            GameObject sign1 = new("Sign 1");
            sign1.transform.parent = markers.transform;
            sign1.transform.localPosition = new(-8, 5, 0);
            sign1.transform.localEulerAngles = new(0, 90, 0);
            var signmark1 = sign1.AddComponent<TechDataCenterMarkerProducer>();
            signmark1.m_markerDataBlockID = 9;

            GameObject sign2 = new("Sign 2");
            sign2.transform.parent = markers.transform;
            sign2.transform.localPosition = new(8, 5, 0);
            sign2.transform.localEulerAngles = new(0, 270, 0);
            var signmark2 = sign2.AddComponent<TechDataCenterMarkerProducer>();
            signmark2.m_markerDataBlockID = 9;

            for (int i = -24; i < 25; i++)
            {
                GameObject tmpmark = new($"Marker {i} west");
                tmpmark.transform.parent = markers.transform;
                tmpmark.transform.localPosition = new(-7.9f, 0, i);
                tmpmark.transform.localEulerAngles = new(0, 90, 0);
                var tmplock = tmpmark.AddComponent<TechDataCenterMarkerProducer>();
                tmplock.m_groupData = mgroup;
                tmplock.m_allowFunction = true;
                tmplock.m_markerDataBlockID = 72;

                GameObject tmpmark2 = new($"Marker {i} east");
                tmpmark2.transform.parent = markers.transform;
                tmpmark2.transform.localPosition = new(7.9f, 0, i);
                tmpmark2.transform.localEulerAngles = new(0, 270, 0);
                var tmplock2 = tmpmark2.AddComponent<TechDataCenterMarkerProducer>();
                tmplock2.m_groupData = mgroup;
                tmplock2.m_allowFunction = true;
                tmplock2.m_markerDataBlockID = 72;
            }

            foreach (var blurgh in EntryPoint.hirnuigo.GetComponentsInChildren<Transform>()) blurgh.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
