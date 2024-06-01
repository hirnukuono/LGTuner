using Expedition;
using GameData;
using GTFO.API.JSON;
using LevelGeneration;
using LGTuner.Configs;
using LGTuner.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LGTuner
{
    public static class BuilderInfo
    {
        public static IEnumerable<Complex> ExtraComplexResourceToLoad => _extraComplexResourceToLoad;
        private static readonly List<Complex> _extraComplexResourceToLoad = new();

        public static ComplexResourceSetDataBlock ComplexResourceSet { get; private set; }
        public static int GridSize { get; private set; }
        public static int GridCenter { get; private set; }
        public static int GridZeroOffset { get; private set; }
        public static float AltitudeOffset { get; private set; }


        public static LayoutConfig MainLayer { get; private set; }
        public static LayoutConfig SecondaryLayer { get; private set; }
        public static LayoutConfig ThirdLayer { get; private set; }
        public static LayoutConfig[] DimensionLayer { get; private set; } = Array.Empty<LayoutConfig>();

        internal static void OnResourceSetSelected()
        {
            ComplexResourceSet = Builder.ComplexResourceSetBlock;
            GridSize = ComplexResourceSet.LevelGenConfig.GridSize;
            GridCenter = GridSize / 2;
            GridZeroOffset = -GridCenter;
            AltitudeOffset = ComplexResourceSet.LevelGenConfig.AltitudeOffset;

            MainLayer = null;
            SecondaryLayer = null;
            ThirdLayer = null;
            DimensionLayer = new LayoutConfig[Enum.GetValues(typeof(eDimensionIndex)).Length];

            _extraComplexResourceToLoad.Clear();

            var activeExp = RundownManager.ActiveExpedition;
            if (ConfigManager.TryGetConfig(activeExp.LevelLayoutData, out var mainLayer))
                MainLayer = mainLayer;

            if (activeExp.SecondaryLayerEnabled && ConfigManager.TryGetConfig(activeExp.SecondaryLayout, out var secLayer))
                SecondaryLayer = secLayer;

            if (activeExp.ThirdLayerEnabled && ConfigManager.TryGetConfig(activeExp.ThirdLayout, out var thirdLayer))
                ThirdLayer = thirdLayer;

            foreach (var dim in Builder.CurrentFloor.m_dimensions)
            {
                if (dim.IsMainDimension || dim.DimensionData.IsStaticDimension)
                    continue;

                var layout = LevelLayoutDataBlock.GetBlock(dim.DimensionData.LevelLayoutData);
                if (layout != null && ConfigManager.TryGetConfig(layout.persistentID, out var dimLayer))
                {
                    Logger.Info("loysimme dimension ja levellayoutin");
                    dimLayer.Reset(dim.Grid.m_sizeX);
                    DimensionLayer[(int)dim.DimensionIndex] = dimLayer;
                }
            }

            MainLayer?.Reset(GridSize);
            SecondaryLayer?.Reset(GridSize);
            ThirdLayer?.Reset(GridSize);

            AddExtraShard(MainLayer);
            AddExtraShard(SecondaryLayer);
            AddExtraShard(ThirdLayer);
            Array.ForEach(DimensionLayer, x => AddExtraShard(x));
        }

        private static void AddExtraShard(LayoutConfig layerConfig)
        {
            if (layerConfig == null)
                return;

            foreach (var type in layerConfig.ExtraComplexResourceToLoad)
            {
                if (!_extraComplexResourceToLoad.Contains(type))
                    _extraComplexResourceToLoad.Add(type);
            }
        }

        public static bool TryGetConfig(LG_Zone zone, out LayoutConfig config)
        {
            if (!Dimension.GetDimension(zone.DimensionIndex, out var dimension))
            {
                Logger.Info("dimension getter failed");
                config = null;
                return false;
            }

            if (dimension.IsMainDimension)
            {
                switch (zone.Layer.m_type)
                {
                    case LG_LayerType.MainLayer:
                        config = MainLayer;
                        return MainLayer != null;

                    case LG_LayerType.SecondaryLayer:
                        config = SecondaryLayer;
                        return SecondaryLayer != null;

                    case LG_LayerType.ThirdLayer:
                        config = ThirdLayer;
                        return ThirdLayer != null;
                }
            }
            else if (!dimension.DimensionData.IsStaticDimension)
            {
                foreach (var dim in Builder.CurrentFloor.m_dimensions)
                {
                    if (dim.IsMainDimension || dim.DimensionData.IsStaticDimension)
                        continue;

                    var layout = LevelLayoutDataBlock.GetBlock(dim.DimensionData.LevelLayoutData);
                    if (layout != null && ConfigManager.TryGetConfig(layout.persistentID, out var dimLayer))
                    {
                        Logger.Info("found a dimension + levellayout");
                        dimLayer.Reset(dim.Grid.m_sizeX);
                        DimensionLayer[(int)dim.DimensionIndex] = dimLayer;
                    }
                }
                config = DimensionLayer[(int)zone.DimensionIndex];
                return config != null;
            }

            config = null;
            return false;
        }

        public static GameObject GetRandomPlug(uint seed, int plugHeight, SubComplex subcomplex, bool withGate)
        {
            var res = ComplexResourceSet;
            return plugHeight switch
            {
                0 => res.GetRandomPrefab(seed, withGate ? res.m_straightPlugsWithGates : res.m_straightPlugsNoGates, subcomplex),
                1 => res.GetRandomPrefab(seed, withGate ? res.m_singleDropPlugsWithGates : res.m_singleDropPlugsNoGates, subcomplex),
                2 => res.GetRandomPrefab(seed, withGate ? res.m_doubleDropPlugsWithGates : res.m_doubleDropPlugsNoGates, subcomplex),
                _ => null,
            };
        }

        public static uint GetLayoutIdOfZone(LG_Zone zone)
        {
            var dimension = zone.Dimension;

            uint levelLayout;
            if (dimension.IsMainDimension)
            {
                var layerType = zone.Layer.m_type;
                var activeExp = RundownManager.ActiveExpedition;
                levelLayout = layerType switch
                {
                    LG_LayerType.MainLayer => activeExp.LevelLayoutData,
                    LG_LayerType.SecondaryLayer => activeExp.SecondaryLayout,
                    LG_LayerType.ThirdLayer => activeExp.ThirdLayout,
                    _ => throw new NotSupportedException($"LayerType: {layerType} is not supported!")
                };
            }
            else
            {
                levelLayout = dimension.DimensionData.LevelLayoutData;
            }

            return levelLayout;
        }
    }
}
