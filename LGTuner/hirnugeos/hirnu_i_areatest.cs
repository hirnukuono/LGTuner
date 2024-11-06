using UnityEngine;
using AIGraph;
using LevelGeneration;
using Expedition;
using GTFO.API;
using GameData;
using FluffyUnderware.DevTools.Extensions;

namespace LGTuner.hirnugeos
{
    public class Hirnu_i_areatest : MonoBehaviour
    {
        public static void OnFactoryBuildDone()
        {
            if (EntryPoint.hirnuiareatestgo != null) EntryPoint.hirnuiareatestgo.SetActive(false);
        }
        public static void OnFactoryBuildStart(SubComplex subcomplex, LG_Zone zone)
        {
            //mining floor + wall "BuildingPart_StorageGround_Gravel_2x2_a"
            // tech floor        "BuildingPart_Lab_Floor_4x4_a"
            //tech wall         "BuildingPart_Lab_Wall_4x8_solid_a"
            //service floor +wall "ServiceCorridor_Ground_6x6_edgeup"

            EntryPoint.hirnuiareatestgo = null;

            EntryPoint.hirnuiareatestgo = new();
            Debug.Log("LGTuner - hirnugeos - building area test tile");
            EntryPoint.hirnuiareatestgo.SetActive(true);
            EntryPoint.hirnuiareatestgo.hideFlags = HideFlags.HideAndDontSave;
            EntryPoint.hirnuiareatestgo.layer = LayerManager.LAYER_DEFAULT;
            EntryPoint.hirnuiareatestgo.name = "hirnu_tile_areatest";
            var geo = EntryPoint.hirnuiareatestgo.AddComponent<LG_Geomorph>();
            var vnv = EntryPoint.hirnuiareatestgo.AddComponent<AIG_VoxelNodeVolume>();

            GameObject area1 = new("Area A");
            area1.transform.parent = EntryPoint.hirnuiareatestgo.transform;
            area1.name = "hirnu_tile_i_a";
            GameObject graphsource = new("AreaAIGraphSource");
            graphsource.transform.parent = area1.transform;
            graphsource.transform.localPosition = new Vector3(0, 2f, -23);
            var tmp2 = graphsource.AddComponent<LG_AreaAIGraphSource>();
            // tmp2.m_position = new(0, 2f, -23);
            var areaa = area1.AddComponent<LG_Area>();
            areaa.m_size = LG_AreaSize.Medium;
            areaa.m_geomorph = geo;
            areaa.m_groupSource = tmp2;

            GameObject subgate = new("SubGate AB");
            subgate.transform.parent = area1.transform;
            subgate.transform.localPosition = new(0, 0, 0);
            subgate.transform.localScale = new(1, 1, 1);
            var weak = subgate.AddComponent<LG_InternalGate>();
            //weak.m_originalForward = new(0, 0, 1);
            weak.m_type = LG_GateType.Medium;
            //weak.m_hasOriginalForward = true;
            //weak.m_hasProgressionSourceArea = true;
            //weak.m_hasProgressionSourceDirection = true;
            //weak.m_linksFrom = areaa;
            weak.m_subComplex = subcomplex;

            GameObject area2 = new("Area B");
            area2.transform.parent = EntryPoint.hirnuiareatestgo.transform;
            area2.name = "hirnu_tile_i_b";
            GameObject graphsource2 = new("AreaAIGraphSource");
            graphsource2.transform.parent = area2.transform;
            graphsource2.transform.localPosition = new Vector3(0, 2f, 23);
            var tmp3 = graphsource2.AddComponent<LG_AreaAIGraphSource>();
            // tmp3.m_position = new(0, 2f, 23);
            var areab = area2.AddComponent<LG_Area>();
            areab.m_size = LG_AreaSize.Medium;
            areab.m_geomorph = geo;
            areab.m_groupSource = tmp3;

            geo.m_areas = new[] { areaa, areab };
            weak.m_linksTo = areab;

            GameObject bg = new("Gate 1");
            bg.transform.parent = areab.transform;
            bg.transform.localPosition = new Vector3(0, 0, 32);
            bg.transform.localEulerAngles = new(0, 0, 0);
            var bgg = bg.AddComponent<LG_Plug>();
            bgg.m_originalForward = new(0, 0, 1);
            bgg.m_hasOriginalForward = true;
            bgg.m_subComplex = subcomplex;
            GameObject sg = new("Gate 2");
            sg.transform.parent = areaa.transform;
            sg.transform.localPosition = new Vector3(0, 0, -32);
            sg.transform.localEulerAngles = new(0, 180, 0);
            var sgg = sg.AddComponent<LG_Plug>();
            sgg.m_originalForward = new(0, 0, -1);
            sgg.m_hasOriginalForward = true;
            sgg.m_subComplex = subcomplex;

            GameObject propsa = new("EnvProps");
            propsa.transform.parent = areaa.transform;
            propsa.transform.localPosition = new(0, 0, 0);

            GameObject propsb = new("EnvProps");
            propsb.transform.parent = areab.transform;
            propsb.transform.localPosition = new(0, 0, 0);

            // floor a
            for (int i = -30; i < 32; i = i + 4) for (int j = -30; j < -1; j = j + 4)
                {
                    if (i < -24 && j > -8 && j < 8) continue;
                    if (i > 24 && j > -8 && j < 8) continue;
                    if (j < -24 && i > -8 && i < 8) continue;
                    if (j > 24 && i > -8 && i < 8) continue;
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    floor.name = "hirnu_floor";
                    floor.transform.parent = propsa.transform;
                    floor.transform.localPosition = new(i, -0.05f, j);
                    floor.transform.localEulerAngles = new(90, 0, 0); // 90 90 0
                    floor.transform.localScale = new Vector3(4, 4, 4);
                }

            // floor b
            for (int i = -30; i < 32; i = i + 4) for (int j = 2; j < 32; j = j + 4)
                {
                    if (i < -24 && j > -8 && j < 8) continue;
                    if (i > 24 && j > -8 && j < 8) continue;
                    if (j < -24 && i > -8 && i < 8) continue;
                    if (j > 24 && i > -8 && i < 8) continue;
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    floor.name = "hirnu_floor";
                    floor.transform.parent = propsb.transform;
                    floor.transform.localPosition = new(i, -0.05f, j);
                    floor.transform.localEulerAngles = new(90, 0, 0); // 90 90 0
                    floor.transform.localScale = new Vector3(4, 4, 4);
                }

            // ceiling a
            if (!zone.Dimension.DimensionData.IsOutside)
            {
                GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Quad);
                ceiling.transform.parent = propsa.transform;
                ceiling.transform.localPosition = new(0, 8, -16);
                ceiling.transform.localEulerAngles = new(270, 0, 0);
                ceiling.transform.localScale = new Vector3(64, 32, 32);
                ceiling.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Texture2D.blackTexture);
            }

            // ceiling b
            if (!zone.Dimension.DimensionData.IsOutside)
            {
                GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Quad);
                ceiling.transform.parent = propsb.transform;
                ceiling.transform.localPosition = new(0, 8, 16);
                ceiling.transform.localEulerAngles = new(270, 0, 0);
                ceiling.transform.localScale = new Vector3(64, 32, 32);
                ceiling.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Texture2D.blackTexture);
            }

            // lights

            GameObject lightsa = new("Lights");
            lightsa.transform.parent = areaa.transform;
            lightsa.transform.localPosition = new(0, 0, 0);

            for (int i = -24; i < 0; i = i + 16)
            {
                GameObject tmplamppu = Instantiate(EntryPoint.hirnulamp.gameObject);
                GameObject l1 = new($"Light {i} west");
                l1.transform.parent = lightsa.transform;
                l1.transform.localPosition = new(-7.9f, 7, i);
                l1.transform.localEulerAngles = new(300, 270, 0);
                tmplamppu.transform.parent = l1.transform;
                tmplamppu.transform.localPosition = new(0, 0, 0);
                tmplamppu.transform.localEulerAngles = new(0, 0, 0);

                GameObject tmplamppu2 = Instantiate(EntryPoint.hirnulamp.gameObject);
                GameObject l2 = new($"Light {i} east");
                l2.transform.parent = lightsa.transform;
                l2.transform.localPosition = new(7.9f, 7, i);
                l2.transform.localEulerAngles = new(300, 90, 0);
                tmplamppu2.transform.parent = l2.transform;
                tmplamppu2.transform.localPosition = new(0, 0, 0);
                tmplamppu2.transform.localEulerAngles = new(0, 0, 0);
            }

            GameObject lightsb = new("Lights");
            lightsb.transform.parent = areab.transform;
            lightsb.transform.localPosition = new(0, 0, 0);

            for (int i = 8; i < 32; i = i + 16)
            {
                GameObject tmplamppu = Instantiate(EntryPoint.hirnulamp.gameObject);
                GameObject l1 = new($"Light {i} west");
                l1.transform.parent = lightsb.transform;
                l1.transform.localPosition = new(-7.9f, 7, i);
                l1.transform.localEulerAngles = new(300, 270, 0);
                tmplamppu.transform.parent = l1.transform;
                tmplamppu.transform.localPosition = new(0, 0, 0);
                tmplamppu.transform.localEulerAngles = new(0, 0, 0);

                GameObject tmplamppu2 = Instantiate(EntryPoint.hirnulamp.gameObject);
                GameObject l2 = new($"Light {i} east");
                l2.transform.parent = lightsb.transform;
                l2.transform.localPosition = new(7.9f, 7, i);
                l2.transform.localEulerAngles = new(300, 90, 0);
                tmplamppu2.transform.parent = l2.transform;
                tmplamppu2.transform.localPosition = new(0, 0, 0);
                tmplamppu2.transform.localEulerAngles = new(0, 0, 0);
            }

            // walls
            GameObject wallw1 = new GameObject("fd_wall_w1");
            GameObject walle1 = new GameObject("fd_wall_e1");
            GameObject wallw2 = new GameObject("fd_wall_w2");
            GameObject walle2 = new GameObject("fd_wall_e2");

            GameObject walls1 = new GameObject("fd_wall_s1");
            GameObject walls2 = new GameObject("fd_wall_s2");
            GameObject walln1 = new GameObject("fd_wall_n1");
            GameObject walln2 = new GameObject("fd_wall_n2");
            GameObject walltop1 = new GameObject("fd_wall_top1");
            GameObject walltop2 = new GameObject("fd_wall_top2");

            wallw1.name = "fd_wall_w1";
            wallw1.transform.SetParent(propsa.transform);
            wallw1.transform.localPosition = new(-8, 4, -16);
            wallw1.transform.localEulerAngles = new(0, 270, 0);

            for (int i = -14; i < 15; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = wallw1.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            walle1.name = "fd_wall_e1";
            walle1.transform.SetParent(propsa.transform);
            walle1.transform.localPosition = new(8, 4, -16);
            walle1.transform.localEulerAngles = new(0, 90, 0);
            for (int i = -14; i < 15; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = walle1.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            wallw2.name = "fd_wall_w2";
            wallw2.transform.SetParent(propsb.transform);
            wallw2.transform.localPosition = new(-8, 4, 16);
            wallw2.transform.localEulerAngles = new(0, 270, 0);

            for (int i = -14; i < 15; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = wallw2.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            walle2.name = "fd_wall_e2";
            walle2.transform.SetParent(propsb.transform);
            walle2.transform.localPosition = new(8, 4, 16);
            walle2.transform.localEulerAngles = new(0, 90, 0);
            for (int i = -14; i < 15; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = walle2.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            walls1.name = "fd_wall_s1";
            walls1.transform.SetParent(propsa.transform);
            walls1.transform.localPosition = new(-6, 4, -0.2f);
            walls1.transform.localEulerAngles = new(0, 0, 0);
            var walls1tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            walls1tmp.name = "hirnu_wall";
            walls1tmp.transform.parent = walls1.transform;
            walls1tmp.transform.localScale = new(4, 8.2f, 4);
            walls1tmp.transform.localPosition = new(0,0,0);

            walls2.name = "fd_wall_s2";
            walls2.transform.SetParent(propsa.transform);
            walls2.transform.localPosition = new(6, 4, -0.2f);
            walls2.transform.localEulerAngles = new(0, 0, 0);
            var walls2tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            walls2tmp.name = "hirnu_wall";
            walls2tmp.transform.parent = walls2.transform;
            walls2tmp.transform.localScale = new(4, 8.2f, 4);
            walls2tmp.transform.localPosition = new(0, 0, 0);

            walln1.name = "fd_wall_n1";
            walln1.transform.SetParent(propsb.transform);
            walln1.transform.localPosition = new(-6, 4, 0.2f);
            walln1.transform.localEulerAngles = new(0, 180, 0);
            var walln1tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            walln1tmp.name = "hirnu_wall";
            walln1tmp.transform.parent = walln1.transform;
            walln1tmp.transform.localScale = new(4, 8.2f, 4);
            walln1tmp.transform.localPosition = new(0, 0, 0);
            walln1tmp.transform.localEulerAngles = new(0, 0, 0);

            walln2.name = "fd_wall_n2";
            walln2.transform.SetParent(propsb.transform);
            walln2.transform.localPosition = new(6, 4, 0.2f);
            walln2.transform.localEulerAngles = new(0, 180, 0);
            var walln2tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            walln2tmp.name = "hirnu_wall";
            walln2tmp.transform.parent = walln2.transform;
            walln2tmp.transform.localScale = new(4, 8.2f, 4);
            walln2tmp.transform.localPosition = new(0, 0, 0);
            walln2tmp.transform.localEulerAngles = new(0, 0, 0);

            walltop1.name = "fd_wall_top1";
            walltop1.transform.SetParent(propsa.transform);
            walltop1.transform.localPosition = new(0, 6, -0.2f);
            walltop1.transform.localEulerAngles = new(0, 0, 0);
            var walltop1tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            walltop1tmp.name = "hirnu_wall";
            walltop1tmp.transform.parent = walltop1.transform;
            walltop1tmp.transform.localScale = new(8, 4, 8);
            walltop1tmp.transform.localPosition = new(0, 0, 0);

            walltop2.name = "fd_wall_top2";
            walltop2.transform.SetParent(propsb.transform);
            walltop2.transform.localPosition = new(0, 6, 0.2f);
            walltop2.transform.localEulerAngles = new(0, 180, 0);
            var walltop2tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            walltop2tmp.name = "hirnu_wall";
            walltop2tmp.transform.parent = walltop2.transform;
            walltop2tmp.transform.localScale = new(8, 4, 8);
            walltop2tmp.transform.localPosition = new(0, 0, 0);
            walltop2tmp.transform.localEulerAngles = new(0, 0, 0);

            // markers
            MarkerGroupDataBlock mgroup = MarkerGroupDataBlock.GetBlock("Tech_DataCenter");
            GameObject markersa = new("Markers");
            markersa.transform.parent = areaa.transform;

            // fallback
            GameObject fb1 = new("Fallback 1");
            fb1.transform.parent = markersa.transform;
            var fbm1 = fb1.AddComponent<TechDataCenterMarkerProducer>();
            fbm1.m_markerDataBlockID = 63;

            // signs
            GameObject sign1 = new("Sign 1");
            sign1.transform.parent = markersa.transform;
            sign1.transform.localPosition = new(-8, 5, -16);
            sign1.transform.localEulerAngles = new(0, 90, 0);
            var signmark1 = sign1.AddComponent<TechDataCenterMarkerProducer>();
            signmark1.m_markerDataBlockID = 9;

            GameObject sign2 = new("Sign 2");
            sign2.transform.parent = markersa.transform;
            sign2.transform.localPosition = new(8, 5, -16);
            sign2.transform.localEulerAngles = new(0, 270, 0);
            var signmark2 = sign2.AddComponent<TechDataCenterMarkerProducer>();
            signmark2.m_markerDataBlockID = 9;

            GameObject markersb = new("Markers");
            markersb.transform.parent = areab.transform;

            GameObject sign3 = new("Sign 1");
            sign3.transform.parent = markersb.transform;
            sign3.transform.localPosition = new(-8, 5, 16);
            sign3.transform.localEulerAngles = new(0, 90, 0);
            var signmark3 = sign3.AddComponent<TechDataCenterMarkerProducer>();
            signmark3.m_markerDataBlockID = 9;

            GameObject sign4 = new("Sign 2");
            sign4.transform.parent = markersb.transform;
            sign4.transform.localPosition = new(8, 5, 16);
            sign4.transform.localEulerAngles = new(0, 270, 0);
            var signmark4 = sign4.AddComponent<TechDataCenterMarkerProducer>();
            signmark4.m_markerDataBlockID = 9;

            for (int i = -24; i < -2; i++)
            {
                GameObject tmpmark = new($"Marker {i} west");
                tmpmark.transform.parent = markersa.transform;
                tmpmark.transform.localPosition = new(-7.9f, 0, i);
                tmpmark.transform.localEulerAngles = new(0, 90, 0);
                var tmplock = tmpmark.AddComponent<TechDataCenterMarkerProducer>();
                tmplock.m_groupData = mgroup;
                tmplock.m_allowFunction = true;
                tmplock.m_markerDataBlockID = 72;

                GameObject tmpmark2 = new($"Marker {i} east");
                tmpmark2.transform.parent = markersa.transform;
                tmpmark2.transform.localPosition = new(7.9f, 0, i);
                tmpmark2.transform.localEulerAngles = new(0, 270, 0);
                var tmplock2 = tmpmark2.AddComponent<TechDataCenterMarkerProducer>();
                tmplock2.m_groupData = mgroup;
                tmplock2.m_allowFunction = true;
                tmplock2.m_markerDataBlockID = 72;
            }

            for (int i = 2; i < 24; i++)
            {
                GameObject tmpmark = new($"Marker {i} west");
                tmpmark.transform.parent = markersb.transform;
                tmpmark.transform.localPosition = new(-7.9f, 0, i);
                tmpmark.transform.localEulerAngles = new(0, 90, 0);
                var tmplock = tmpmark.AddComponent<TechDataCenterMarkerProducer>();
                tmplock.m_groupData = mgroup;
                tmplock.m_allowFunction = true;
                tmplock.m_markerDataBlockID = 72;

                GameObject tmpmark2 = new($"Marker {i} east");
                tmpmark2.transform.parent = markersb.transform;
                tmpmark2.transform.localPosition = new(7.9f, 0, i);
                tmpmark2.transform.localEulerAngles = new(0, 270, 0);
                var tmplock2 = tmpmark2.AddComponent<TechDataCenterMarkerProducer>();
                tmplock2.m_groupData = mgroup;
                tmplock2.m_allowFunction = true;
                tmplock2.m_markerDataBlockID = 72;
            }

            foreach (var blurgh in EntryPoint.hirnuiareatestgo.GetComponentsInChildren<Transform>()) blurgh.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
