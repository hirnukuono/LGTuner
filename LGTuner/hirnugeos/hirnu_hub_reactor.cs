using UnityEngine;
using LevelGeneration;
using Expedition;
using GTFO.API;
using System.Collections.Generic;
using GameData;
using static Il2CppSystem.DateTimeParse;

namespace LGTuner.hirnugeos
{
    public class Hirnu_hub_reactor : MonoBehaviour
    {
        public static void OnFactoryBuildDone()
        {
            if (EntryPoint.hirnuhubreactorgo != null) EntryPoint.hirnuhubreactorgo.SetActive(false);
        }
        public static void OnFactoryBuildStart(SubComplex subcomplex, LG_Zone zone)
        {
            EntryPoint.hirnuhubreactorgo = null;
            if (EntryPoint.hirnureactorterminal == null) EntryPoint.hirnureactorterminal = AssetAPI.GetLoadedAsset("Assets/AssetPrefabs/Complex/Generic/FunctionMarkers/Terminal_Floor.prefab").Cast<GameObject>();

            EntryPoint.hirnuhubreactorgo = new();
            EntryPoint.hirnuhubreactorgo.SetActive(true);
            Debug.Log("LGTuner - hirnugeos - building hub-reactor-tile");
            EntryPoint.hirnuhubreactorgo.SetActive(true);
            EntryPoint.hirnuhubreactorgo.hideFlags = HideFlags.HideAndDontSave;
            EntryPoint.hirnuhubreactorgo.layer = LayerManager.LAYER_DEFAULT;
            EntryPoint.hirnuhubreactorgo.name = "hirnu_tile_hub_reactor";
            var geo = EntryPoint.hirnuhubreactorgo.AddComponent<LG_Geomorph>();
            geo.m_assignedSubComplexFromZone = subcomplex;
            //var vnv = EntryPoint.hirnuhubreactorgo.AddComponent<AIG_VoxelNodeVolume>();
            var rea = EntryPoint.hirnuhubreactorgo.AddComponent<LG_WardenObjective_Reactor>();
            var ter = EntryPoint.hirnuhubreactorgo.AddComponent<LG_GenericTerminalItem>();
            GameObject area = new("Area A");
            area.transform.parent = EntryPoint.hirnuhubreactorgo.transform;
            area.name = "Area A";
            GameObject graphsource = new("AIGraphSource");
            graphsource.transform.parent = area.transform;
            graphsource.transform.localPosition += new Vector3(-5, 2f, 0);
            var tmp2 = graphsource.AddComponent<LG_AreaAIGraphSource>();
            tmp2.m_position = new(0, 2f, 0);
            var area2 = area.AddComponent<LG_Area>();
            area2.m_size = LG_AreaSize.Medium;
            area2.m_geomorph = geo;
            geo.m_areas = new[] { area2 };
            GameObject left = new("Gate 1");
            left.transform.parent = area.transform;
            left.transform.localPosition = new Vector3(-32, 0, 0);
            left.transform.localEulerAngles = new(0, 270.0002f, 0);
            var gate1 = left.AddComponent<LG_Plug>();
            gate1.m_subComplex = subcomplex;
            GameObject up = new("Gate 2");
            up.transform.parent = area.transform;
            up.transform.localPosition = new Vector3(0, 0, 32);
            up.transform.localEulerAngles = new(0, 0.0002f, 0);
            var gate2 = up.AddComponent<LG_Plug>();
            gate2.m_originalForward = new(0, 0, 1);
            gate2.m_hasOriginalForward = true;
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
            down.transform.localEulerAngles = new(0, 179.9999f, 0);
            var gate4 = down.AddComponent<LG_Plug>();
            gate4.m_originalForward = new(0, 0, -1);
            gate4.m_hasOriginalForward = true;
            gate4.m_subComplex = subcomplex;

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
            fb1.transform.localPosition = new(0, 0, -22);
            var fbm1 = fb1.AddComponent<TechDataCenterMarkerProducer>();
            fbm1.m_markerDataBlockID = 63;

            GameObject termalign = new("Terminal_Align");
            termalign.transform.parent = area.transform;
            termalign.transform.localPosition = new(0, 0, -2.7f);
            termalign.transform.localEulerAngles = new(0, 180, 0);

            GameObject scanalign = new("SecurityScan_Align");
            scanalign.transform.parent = area.transform;
            scanalign.transform.localPosition = new(0, 0, -8f);

            GameObject reasound = new("Reactor_SoundEmitter");
            reasound.transform.parent = area.transform;
            reasound.transform.localPosition = new(0, 2.09f, 0);

            rea.m_currentState = new() { status = eReactorStatus.Inactive_Idle };
            rea.m_reactorArea = area2;
            rea.m_chainedPuzzleAlign = scanalign.transform;
            rea.m_chainedPuzzleAlignMidObjective = scanalign.transform;

            rea.m_terminalAlign = termalign.transform;
            rea.m_terminalItemComp = ter;
            rea.m_terminalPrefab = EntryPoint.hirnureactorterminal;

            for (int i = -2; i < 3; i++)
            {
                GameObject tmpmark = new($"Marker {i} west");
                tmpmark.transform.parent = markers.transform;
                tmpmark.transform.localPosition = new(-2.5f, 0, i);
                tmpmark.transform.localEulerAngles = new(0, 270, 0);
                var tmplock = tmpmark.AddComponent<TechDataCenterMarkerProducer>();
                tmplock.m_groupData = mgroup;
                tmplock.m_allowFunction = true;
                tmplock.m_markerDataBlockID = 72;

                GameObject tmpmark2 = new($"Marker {i} north");
                tmpmark2.transform.parent = markers.transform;
                tmpmark2.transform.localPosition = new(i, 0, 2.5f);
                tmpmark2.transform.localEulerAngles = new(0, 0, 0);
                var tmplock2 = tmpmark2.AddComponent<TechDataCenterMarkerProducer>();
                tmplock2.m_groupData = mgroup;
                tmplock2.m_allowFunction = true;
                tmplock2.m_markerDataBlockID = 72;

                GameObject tmpmark3 = new($"Marker {i} east");
                tmpmark3.transform.parent = markers.transform;
                tmpmark3.transform.localPosition = new(2.5f, 0, i);
                tmpmark3.transform.localEulerAngles = new(0, 90, 0);
                var tmplock3 = tmpmark3.AddComponent<TechDataCenterMarkerProducer>();
                tmplock3.m_groupData = mgroup;
                tmplock3.m_allowFunction = true;
                tmplock3.m_markerDataBlockID = 72;
            }


            // floor

            for (int i = -30; i < 32; i = i + 4) for (int j = -30; j < 32; j = j + 4)
                {
                    if (i < -24 && j > -8 && j < 8) continue;
                    if (i > 24 && j > -8 && j < 8) continue;
                    if (j < -24 && i > -8 && i < 8) continue;
                    if (j > 24 && i > -8 && i < 8) continue;
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    floor.name = "hirnu_floor";
                    floor.transform.parent = props.transform;
                    floor.transform.localPosition = new(i, -0.05f, j);
                    floor.transform.localEulerAngles = new(90, 0, 0); // 90 90 0
                    floor.transform.localScale = new Vector3(4, 4, 4);
                }

            //area21.m_bounds = floor.GetComponent<MeshFilter>().mesh.bounds;

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

            // walls
            GameObject wallsw1 = new();
            GameObject wallnw1 = new();
            GameObject wallne1 = new();
            GameObject wallse1 = new();

            wallsw1.name = "hirnu_wall_sw1";
            wallsw1.transform.SetParent(props.transform);
            wallsw1.transform.localPosition = new(-20, 4, -20);
            wallsw1.transform.localEulerAngles = new(0, 225, 0);

            for (float i = -14.97f; i < 17; i += 3.394f)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = wallsw1.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(3.394f, 8, 3.394f);
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
                tmp.name = "hirnu_wall";
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
                tmp.name = "hirnu_wall";
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
                tmp.name = "hirnu_wall";
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

            // center
            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cube);
            center.name = "hirnu_wall";
            center.transform.parent = props.transform;
            center.transform.localPosition = new(0, 4, 0);
            center.transform.localScale = new(5, 8.2f, 5);

            // lights
            GameObject lights = new("Lights");
            lights.transform.parent = area.transform;
            lights.transform.localPosition = new(0, 0, 0);

            Dictionary<Vector3, int> lightpos = new();
            lightpos[new(-2.5f, 7, 0)] = 90;
            lightpos[new(0, 7, 2.5f)] = 180;
            lightpos[new(2.5f, 7, 0)] = 270;
            lightpos[new(0, 7, -2.5f)] = 0;

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

            foreach (var blurgh in EntryPoint.hirnuhubreactorgo.GetComponentsInChildren<Transform>()) blurgh.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}