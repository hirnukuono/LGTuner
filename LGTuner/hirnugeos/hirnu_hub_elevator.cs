using UnityEngine;
using AIGraph;
using LevelGeneration;
using Expedition;
using GTFO.API;
using FluffyUnderware.DevTools.Extensions;
using System.Collections.Generic;
using GameData;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LGTuner.hirnugeos
{
    public class Hirnu_hub_elevator : MonoBehaviour
    {
        public static LG_PrefabSpawner hirnulamp;
        public static GameObject hirnulampfixture;

        public static void OnFactoryBuildDone()
        {
            if (EntryPoint.hirnuhubelevatorgo != null) EntryPoint.hirnuhubelevatorgo.SetActive(false);
        }
        public static void OnFactoryBuildStart(SubComplex subcomplex, bool IsOutside)
        {
            subcomplex = Builder.LayerBuildDatas[0].m_zoneBuildDatas[0].SubComplex;
            Hirnu_materials.OnFactoryBuildStart();

            EntryPoint.hirnuhubelevatorgo = new();
            Debug.Log($"LGTuner - hirnugeos - building hub-elevator-tile, subcomplex {subcomplex}");
            EntryPoint.hirnuhubelevatorgo.SetActive(true);
            EntryPoint.hirnuhubelevatorgo.hideFlags = HideFlags.HideAndDontSave;
            EntryPoint.hirnuhubelevatorgo.layer = LayerManager.LAYER_DEFAULT;
            EntryPoint.hirnuhubelevatorgo.name = "hirnu_tile_hub_elevator";
            var geo = EntryPoint.hirnuhubelevatorgo.AddComponent<LG_FloorTransition>();
            geo.m_shapeType = LG_TileShapeType.s1x1;
            geo.m_transitionType = LG_FloorTransitionType.Elevator;
            var esl = EntryPoint.hirnuhubelevatorgo.AddComponent<ElevatorShaftLanding>();
            var vnv = EntryPoint.hirnuhubelevatorgo.AddComponent<AIG_VoxelNodeVolume>();

            GameObject area = new("Area A");
            area.transform.parent = EntryPoint.hirnuhubelevatorgo.transform;
            area.name = "hirnu_tile_hub_elevator_a";
            GameObject graphsource = new("AreaAIGraphSource");
            graphsource.transform.parent = area.transform;
            graphsource.transform.localPosition += new Vector3(-5, 2f, 0);
            var tmp2 = graphsource.AddComponent<LG_AreaAIGraphSource>();
            tmp2.m_position = new(0, 2f, 0);
            var area2 = area.AddComponent<LG_Area>();
            area2.m_size = LG_AreaSize.Medium;
            area2.m_geomorph = geo;
            geo.m_areas = new[] { area2 };

            // exit
            GameObject exit = area.transform.AddChildGameObject("ExpeditionExitScanAlign");
            exit.transform.localPosition = new(0.32f, 0.4f, 15.27f);

            // cargo
            var tmpelev = AssetAPI.GetLoadedAsset("Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_32x32_elevator_shaft_lab_03.prefab").Cast<GameObject>();
            foreach (var tra in tmpelev.GetComponentsInChildren<Transform>()) if (tra.name == "ElevatorCargoAlign") EntryPoint.hirnucargocage = Instantiate(tra.gameObject);
            GameObject cargo = Instantiate(EntryPoint.hirnucargocage);
            cargo.transform.parent = area.transform;
            cargo.transform.position = new(0, 0, 0);
            cargo.transform.localPosition = new(0, 0, 14);

            esl.m_cargoCageAlign = cargo.transform;
            esl.m_securityScanAlign = exit.transform;

            // spawns
            GameObject spawn = area.transform.AddChildGameObject("spawn");
            spawn.transform.localPosition = new(-3.136f, 1.338f, 13.661f);

            GameObject spawn2 = area.transform.AddChildGameObject("spawn (2)");
            spawn2.transform.localPosition = new(-1.542f, 1.135f, 13.099f);

            GameObject spawn3 = area.transform.AddChildGameObject("spawn (3)");
            spawn3.transform.localPosition = new(1.284f, 1.135f, 13.099f);

            GameObject spawn4 = area.transform.AddChildGameObject("spawn (4)");
            spawn4.transform.localPosition = new(2.89f, 1.405f, 12.08f);

            geo.m_spawnPoints = new(new Transform[] { spawn.transform, spawn2.transform, spawn3.transform, spawn4.transform });

            // gates
            GameObject north = new("Gate");
            north.transform.parent = area.transform;
            north.transform.localPosition = new Vector3(0, 0, 32);
            north.transform.localEulerAngles = new(0, 0, 0);
            GameObject plug = north.transform.AddChildGameObject("plug");
            GameObject cros = plug.transform.AddChildGameObject("crossing");
            GameObject behi = plug.transform.AddChildGameObject("behind");

            // freenodet
            var fn1 = cros.transform.AddChildGameObject("FreeNode 1");
            var fn2 = cros.transform.AddChildGameObject("FreeNode 2");
            var fn3 = cros.transform.AddChildGameObject("FreeNode 3");
            var fn4 = cros.transform.AddChildGameObject("FreeNode 4");
            var fn5 = cros.transform.AddChildGameObject("FreeNode 5");
            var fn6 = cros.transform.AddChildGameObject("FreeNode 6");

            fn1.transform.localPosition = new(-1.528f, 1.16f, 32);
            fn2.transform.localPosition = new(-0.905f, 0.168f, 32);
            fn3.transform.localPosition = new(-0.185f, 0.168f, 32);
            fn4.transform.localPosition = new(0.487f, 0.168f, 32);
            fn5.transform.localPosition = new(1.242f, 0.168f, 32);
            fn6.transform.localPosition = new(1.974f, 0.168f, 32);

            var ffn1 = fn1.AddComponent<AIG_FreeNode>();
            var ffn2 = fn2.AddComponent<AIG_FreeNode>();
            var ffn3 = fn3.AddComponent<AIG_FreeNode>();
            var ffn4 = fn4.AddComponent<AIG_FreeNode>();
            var ffn5 = fn5.AddComponent<AIG_FreeNode>();
            var ffn6 = fn6.AddComponent<AIG_FreeNode>();

            //portalhelper
            var portalhelper = cros.transform.AddChildGameObject("PortalHelper");
            portalhelper.transform.localPosition = new(0, 0, 32);
            portalhelper.transform.localScale = new(16, 8, 1);
            portalhelper.AddComponent<CullingSystem.C_PortalHelper>();

            var gate1 = north.AddComponent<LG_PlugCustom>();
            north.AddComponent<AIG_PlugSocket>();
            var doorinsert = north.AddComponent<AIG_DoorInsert>();
            Il2CppReferenceArray<AIG_FreeNode> freenodelist = new(new AIG_FreeNode[] { ffn1, ffn2, ffn3, ffn4, ffn5, ffn6 });
            doorinsert.m_nodes = freenodelist;

            gate1.m_subComplex = subcomplex;
            gate1.m_linksFrom = area2;

            GameObject up = new("Gate X");
            up.transform.parent = area.transform;
            up.transform.localPosition = new Vector3(0, 0, 32);
            var gatex = up.AddComponent<LG_Plug>();
            gatex.m_subComplex = subcomplex;

            GameObject left = new("Gate 2");
            left.transform.parent = area.transform;
            left.transform.localPosition = new Vector3(-32, 0, 0);
            left.transform.localEulerAngles = new(0, 270.0002f, 0);
            var gate2 = left.AddComponent<LG_Plug>();
            gate2.m_subComplex = subcomplex;

            GameObject right = new("Gate 3");
            right.transform.parent = area.transform;
            right.transform.localPosition = new Vector3(32, 0, 0);
            right.transform.localEulerAngles = new(0, 89.9999f, 0);
            var gate3 = right.AddComponent<LG_Plug>();
            gate3.m_subComplex = subcomplex;

            GameObject down = new("Gate 4");
            down.transform.parent = area.transform;
            down.transform.localPosition = new Vector3(0, 0, -32);
            down.transform.localEulerAngles = new(0, 180, 0);
            var gate4 = down.AddComponent<LG_Plug>();

            geo.m_plugs = new();
            geo.m_plugs.Add(gate1);
            geo.m_plugs.Add(gate2);
            geo.m_plugs.Add(gate3);
            geo.m_plugs.Add(gate4);
            geo.m_plugs.Add(gatex);

            area2.m_groupSource = tmp2;
            area2.m_geomorph = geo;

            GameObject props = new("EnvProps");
            props.transform.parent = area.transform;
            props.transform.localPosition = new(0, 0, 0);

            // markers
            MarkerGroupDataBlock mgroup = MarkerGroupDataBlock.GetBlock("Tech_DataCenter");
            GameObject markers = new("Markers");
            markers.transform.parent = area.transform;

            // fallback
            GameObject fb1 = new("Fallback 1");
            fb1.transform.parent = markers.transform;
            fb1.transform.localPosition = new(0, 0, 22);
            var fbm1 = fb1.AddComponent<TechDataCenterMarkerProducer>();
            fbm1.m_markerDataBlockID = 63;




            // floor

            for (int i = -31; i < 32; i = i + 2) for (int j = -31; j < 32; j = j + 2)
                {
                    if (i < -24 && j > -8 && j < 8) continue;
                    if (i > 24 && j > -8 && j < 8) continue;
                    if (j < -24 && i > -8 && i < 8) continue;
                    if (j > 24 && i > -8 && i < 8) continue;
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    floor.gameObject.name = "hirnu_floor";
                    floor.transform.parent = props.transform;
                    floor.transform.localPosition = new(i, -0.05f, j);
                    floor.transform.localEulerAngles = new(90, 0, 0); // 90 90 0
                    floor.transform.localScale = new Vector3(2, 2, 2);
                }

            // ceiling
            for (int i = -31; i < 32; i = i + 2) for (int j = -31; j < 32; j = j + 2)
                {
                    if (i > -16 && i < 16 && j > -16 && j < 16) continue;
                    GameObject ceil = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    ceil.gameObject.name = "hirnu_wall";
                    ceil.transform.parent = props.transform;
                    ceil.transform.localPosition = new(i, 8, j);
                    ceil.transform.localEulerAngles = new(270, 0, 0); // 90 90 0
                    ceil.transform.localScale = new Vector3(2, 2, 2);
                }

            // north plug 
            GameObject pwallw = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject pwalle = GameObject.CreatePrimitive(PrimitiveType.Quad);

            pwallw.gameObject.name = "hirnu_wall";
            pwallw.transform.parent = cros.transform;
            pwallw.transform.localPosition = new(-8, 4, 32);
            pwallw.transform.localScale = new(1, 8, 1);
            pwallw.transform.localEulerAngles = new(0, 270, 0);

            pwalle.gameObject.name = "hirnu_wall";
            pwalle.transform.parent = cros.transform;
            pwalle.transform.localPosition = new(8, 4, 32);
            pwalle.transform.localScale = new(1, 8, 1);
            pwalle.transform.localEulerAngles = new(0, 90, 0);

            // walls
            GameObject wallsw1 = new();
            GameObject wallnw1 = new();
            GameObject wallne1 = new();
            GameObject wallse1 = new();

            wallsw1.gameObject.name = "hirnu_wall_sw1";
            wallsw1.transform.SetParent(props.transform);
            wallsw1.transform.localPosition = new(-20, 4, -20);
            wallsw1.transform.localEulerAngles = new(0, 225, 0);

            for (float i = -14.97f; i < 17; i += 3.394f)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.transform.name = "hirnu_wall";
                tmp.transform.parent = wallsw1.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(3.394f, 8.2f, 3.394f);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            GameObject sign1 = new("Sign 1");
            sign1.transform.parent = markers.transform;
            sign1.transform.localPosition = new(-20, 5, -20);
            sign1.transform.localEulerAngles = new(0, 45, 0);
            var signmark1 = sign1.AddComponent<TechDataCenterMarkerProducer>();
            signmark1.m_markerDataBlockID = 9;
            signmark1.m_allowFunction = true;

            wallnw1.name = "hirnu_wall_nw1";
            wallnw1.transform.SetParent(props.transform);
            wallnw1.transform.localPosition = new(-20, 4, 20);
            wallnw1.transform.localEulerAngles = new(0, 315, 0);

            for (float i = -14.97f; i < 17; i += 3.394f)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.gameObject.name = "hirnu_wall";
                tmp.transform.parent = wallnw1.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(3.394f, 8.2f, 3.394f);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            GameObject sign2 = new("Sign 2");
            sign2.transform.parent = markers.transform;
            sign2.transform.localPosition = new(-20, 5, 20);
            sign2.transform.localEulerAngles = new(0, 135, 0);
            var signmark2 = sign2.AddComponent<TechDataCenterMarkerProducer>();
            signmark2.m_markerDataBlockID = 9;
            signmark2.m_allowFunction = true;

            wallne1.name = "hirnu_wall_ne1";
            wallne1.transform.SetParent(props.transform);
            wallne1.transform.localPosition = new(20, 4, 20);
            wallne1.transform.localEulerAngles = new(0, 45, 0);

            for (float i = -14.97f; i < 17; i += 3.394f)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.gameObject.name = "hirnu_wall";
                tmp.transform.parent = wallne1.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(3.394f, 8.2f, 3.394f);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            GameObject sign3 = new("Sign 3");
            sign3.transform.parent = markers.transform;
            sign3.transform.localPosition = new(20, 5, 20);
            sign3.transform.localEulerAngles = new(0, 225, 0);
            var signmark3 = sign3.AddComponent<TechDataCenterMarkerProducer>();
            signmark3.m_markerDataBlockID = 9;
            signmark3.m_allowFunction = true;


            wallse1.name = "hirnu_wall_se1";
            wallse1.transform.SetParent(props.transform);
            wallse1.transform.localPosition = new(20, 4, -20);
            wallse1.transform.localEulerAngles = new(0, 135, 0);

            for (float i = -14.97f; i < 17; i += 3.394f)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.gameObject.name = "hirnu_wall";
                tmp.transform.parent = wallse1.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(3.394f, 8.2f, 3.394f);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            GameObject sign4 = new("Sign 4");
            sign4.transform.parent = markers.transform;
            sign4.transform.localPosition = new(20, 5, -20);
            sign4.transform.localEulerAngles = new(0, 315, 0);
            var signmark4 = sign4.AddComponent<TechDataCenterMarkerProducer>();
            signmark4.m_markerDataBlockID = 9;
            signmark4.m_allowFunction = true;

            // upwalls
            GameObject wallupn = new();
            wallupn.transform.SetParent(props.transform);
            wallupn.transform.localPosition = new(0, 16, 16);

            GameObject wallupe = new();
            wallupe.transform.SetParent(props.transform);
            wallupe.transform.localPosition = new(16, 16, 0);
            wallupe.transform.localEulerAngles = new(0, 90, 0);

            GameObject wallupw = new();
            wallupw.transform.SetParent(props.transform);
            wallupw.transform.localPosition = new(-16, 16, 0);
            wallupw.transform.localEulerAngles = new(0, 270, 0);

            GameObject wallups = new();
            wallups.transform.SetParent(props.transform);
            wallups.transform.localPosition = new(0, 16, -16);
            wallups.transform.localEulerAngles = new(0, 180, 0);

            for (int i = -4; i < 13; i = i + 8) for (int j = -14; j < 15; j = j + 4)
                {
                    var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tmp.transform.name = "hirnu_wall";
                    tmp.transform.SetParent(wallupn.transform);
                    tmp.transform.localEulerAngles = new(0, 0, 0);
                    tmp.transform.localScale = new(4, 8, 4);
                    tmp.transform.localPosition = new(j, i, 0);
                }

            for (int i = -4; i < 13; i = i + 8) for (int j = -14; j < 15; j = j + 4)
                {
                    var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tmp.transform.name = "hirnu_wall";
                    tmp.transform.SetParent(wallupe.transform);
                    tmp.transform.localEulerAngles = new(0, 0, 0);
                    tmp.transform.localScale = new(4, 8, 4);
                    tmp.transform.localPosition = new(j, i, 0);
                }

            for (int i = -4; i < 13; i = i + 8) for (int j = -14; j < 15; j += 4)
                {
                    var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tmp.transform.name = "hirnu_wall";
                    tmp.transform.SetParent(wallupw.transform);
                    tmp.transform.localEulerAngles = new(0, 0, 0);
                    tmp.transform.localScale = new(4, 8, 4);
                    tmp.transform.localPosition = new(j, i, 0);
                }

            for (int i = -4; i < 13; i = i + 8) for (int j = -14; j < 15; j += 4)
                {
                    var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tmp.transform.name = "hirnu_wall";
                    tmp.transform.SetParent(wallups.transform);
                    tmp.transform.localEulerAngles = new(0, 0, 0);
                    tmp.transform.localScale = new(4, 8, 4);
                    tmp.transform.localPosition = new(j, i, 0);
                }

            // lights
            GameObject lights = new("Lights");
            lights.transform.parent = area.transform;
            lights.transform.localPosition = new(0, 0, 0);

            Dictionary<Vector3, int> lightpos = new();
            lightpos[new(-16, 8f, 0f)] = 270;
            lightpos[new(0f, 8f, 16f)] = 0;
            lightpos[new(16f, 8f, 0f)] = 90;
            lightpos[new(0f, 8f, -16f)] = 180;

            foreach (var vec in lightpos)
            {
                GameObject tmplamppu = Instantiate(EntryPoint.hirnulamp.gameObject);
                GameObject l1 = new($"Light {vec.Value}");
                l1.transform.parent = lights.transform;
                l1.transform.localPosition = vec.Key;
                l1.transform.localEulerAngles = new(300, vec.Value, 0);
                tmplamppu.transform.parent = l1.transform;
                tmplamppu.transform.localPosition = new(0, 0, 0);
                tmplamppu.transform.localRotation = new(0, 0, 0, 0);
            }


            // terminal
            
            GameObject termm = new("Terminal 1");
            termm.transform.parent = markers.transform;
            termm.transform.localPosition = new(19.4f, 0, 19.4f);
            termm.transform.localEulerAngles = new(0, 225, 0);
            var termmark = termm.AddComponent<TechDataCenterMarkerProducer>();
            termmark.m_groupData = mgroup;
            termmark.m_allowFunction = true;
            termmark.m_markerDataBlockID = 77;


            foreach (var blurgh in EntryPoint.hirnuhubelevatorgo.GetComponentsInChildren<Transform>()) blurgh.gameObject.hideFlags = HideFlags.HideAndDontSave;
            
        }
    }
}