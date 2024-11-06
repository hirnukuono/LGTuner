using UnityEngine;
using AIGraph;
using LevelGeneration;
using GameData;
using Expedition;
using GTFO.API;
using FluffyUnderware.DevTools.Extensions;

namespace LGTuner.hirnugeos
{
    public class Hirnu_deadend : MonoBehaviour
    {
        public static void OnFactoryBuildDone()
        {
            if (EntryPoint.hirnudeadendgo != null) EntryPoint.hirnudeadendgo.SetActive(false);
        }
        public static void OnFactoryBuildStart(SubComplex subcomplex, LG_Zone zone)
        {
            EntryPoint.hirnudeadendgo = null;
            EntryPoint.hirnudeadendgo = new();
            EntryPoint.hirnudeadendgo.SetActive(true);
            Debug.Log("LGTuner - hirnugeos - building dead-end tile");
            EntryPoint.hirnudeadendgo.SetActive(true);
            EntryPoint.hirnudeadendgo.hideFlags = HideFlags.HideAndDontSave;
            EntryPoint.hirnudeadendgo.layer = LayerManager.LAYER_DEFAULT;
            EntryPoint.hirnudeadendgo.name = "fd_tile_deadend";
            var geo = EntryPoint.hirnudeadendgo.AddComponent<LG_Geomorph>();
            geo.m_drawWalls = true;
            var vnv = EntryPoint.hirnudeadendgo.AddComponent<AIG_VoxelNodeVolume>();

            GameObject area = new("Area A");
            area.transform.parent = EntryPoint.hirnudeadendgo.transform;
            area.name = "fd_tile_deadend_a";
            GameObject graphsource = new("AreaAIGraphSource");
            graphsource.transform.parent = area.transform;
            graphsource.transform.localPosition += new Vector3(0, 2f, -23);
            var tmp2 = graphsource.AddComponent<LG_AreaAIGraphSource>();
            tmp2.m_position = new(0, 2f, 0);
            var area2 = area.AddComponent<LG_Area>();
            area2.m_size = LG_AreaSize.Medium;
            area2.m_geomorph = geo;
            geo.m_areas = new[] { area2 };
            GameObject sg = new("Gate 1");
            sg.transform.parent = area.transform;
            sg.transform.localPosition = new Vector3(0, 0, -32);
            sg.transform.localEulerAngles = new(0, 180, 0);
            var gate1 = sg.AddComponent<LG_Plug>();
            gate1.m_originalForward = new(0, 0, -1);
            gate1.m_hasOriginalForward = true;
            gate1.m_subComplex = subcomplex;

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
                    floor.name = "hirnu_floor";
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

            GameObject tmplamppu = Instantiate(EntryPoint.hirnulamp.gameObject);
            GameObject l1 = new($"Light 1 west");
            l1.transform.parent = lights.transform;
            l1.transform.localPosition = new(-4, 7, -16.1f);
            l1.transform.localEulerAngles = new(300, 0, 0);
            tmplamppu.transform.parent = l1.transform;
            tmplamppu.transform.localPosition = new(0, 0, 0);
            tmplamppu.transform.localRotation = new(0, 0, 0, 0);

            GameObject tmplamppu2 = Instantiate(EntryPoint.hirnulamp.gameObject);
            GameObject l2 = new($"Light 1 east");
            l2.transform.parent = lights.transform;
            l2.transform.localPosition = new(4, 7, -16.1f);
            l2.transform.localEulerAngles = new(300, 0, 0);
            tmplamppu2.transform.parent = l2.transform;
            tmplamppu2.transform.localPosition = new(0, 0, 0);
            tmplamppu2.transform.localRotation = new(0, 0, 0, 0);

            // walls
            GameObject wallw = new();
            GameObject walle = new();
            GameObject walln = new();

            wallw.name = "fd_wall_w";
            wallw.transform.SetParent(props.transform);
            wallw.transform.localPosition = new(-8, 4, -24);
            wallw.transform.localEulerAngles = new(0, 270, 0);
            for (float i = -6; i < 7; i += 4)
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
            walle.transform.localPosition = new(8, 4, -24);
            walle.transform.localEulerAngles = new(0, 90, 0);
            for (float i = -6; i < 7; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = walle.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            walln.name = "fd_wall_n";
            walln.transform.SetParent(props.transform);
            walln.transform.localPosition = new(0, 4, -16);
            walln.transform.localEulerAngles = new(0, 0, 0);
            for (float i = -6; i < 7; i += 4)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tmp.name = "hirnu_wall";
                tmp.transform.parent = walln.transform;
                tmp.transform.localEulerAngles = new(0, 0, 0);
                tmp.transform.localScale = new(4, 8.2f, 4);
                tmp.transform.localPosition = new(i, 0, 0);
            }

            MarkerGroupDataBlock mgroup = MarkerGroupDataBlock.GetBlock("Tech_DataCenter");
            // markers
            GameObject markers = new("Markers");
            markers.transform.parent = area.transform;

            // fallback
            GameObject fb1 = new("Fallback 1");
            fb1.transform.parent = markers.transform;
            fb1.transform.localPosition = new(0, 0, -22);
            var fbm1 = fb1.AddComponent<TechDataCenterMarkerProducer>();
            fbm1.m_markerDataBlockID = 63;

            // sign
            GameObject sign = new("Sign 1");
            sign.transform.parent = markers.transform;
            sign.transform.localPosition = new(0, 5, -16);
            sign.transform.localEulerAngles = new(0, 180, 0);
            var signmark = sign.AddComponent<TechDataCenterMarkerProducer>();
            signmark.m_markerDataBlockID = 9;

            // terminal
            GameObject termm = new("Terminal 1");
            termm.transform.parent = markers.transform;
            termm.transform.localPosition = new(0, 0, -16.1f);
            termm.transform.localEulerAngles = new(0, 180, 0);
            var termmark = termm.AddComponent<TechDataCenterMarkerProducer>();
            termmark.m_groupData = mgroup;
            termmark.m_allowFunction = true;
            termmark.m_markerDataBlockID = 77;
            for (int i = 1; i < 13; i++)
            {
                if (i - 6 == 0) continue;
                GameObject tmpmark = new($"Locker {i}");
                tmpmark.transform.parent = markers.transform;
                tmpmark.transform.localPosition = new(i - 6, 0, -16.1f);
                tmpmark.transform.localEulerAngles = new(0, 180, 0);
                var tmplock = tmpmark.AddComponent<TechDataCenterMarkerProducer>();
                tmplock.m_groupData = mgroup;
                tmplock.m_allowFunction = true;
                tmplock.m_markerDataBlockID = 72;
            }

            foreach (var blurgh in EntryPoint.hirnudeadendgo.GetComponentsInChildren<Transform>()) blurgh.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}