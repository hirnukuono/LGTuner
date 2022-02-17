using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace LGTuner
{
    public enum DirectionType
    {
        Unknown,
        Forward,
        Backward,
        Left,
        Right
    }

    public enum RotateType
    {
        None,
        Flip,
        MoveTo_Left,
        MoveTo_Right,

        Towards_Random,
        Towards_Forward,
        Towards_Backward,
        Towards_Left,
        Towards_Right
    }

    public struct LG_PlugInfo
    {
        public static readonly Quaternion NoRotation = Quaternion.AngleAxis(0f, Vector3.up);
        public static readonly Quaternion RightRotation = Quaternion.AngleAxis(90f, Vector3.up);
        public static readonly Quaternion BackwardRotation = Quaternion.AngleAxis(180f, Vector3.up);
        public static readonly Quaternion LeftRotation = Quaternion.AngleAxis(270f, Vector3.up);

        public static readonly Vector3 FrontPlugPosition = Vector3.forward * 32.0f;
        public static readonly Vector3 BackPlugPosition = Vector3.back * 32.0f;
        public static readonly Vector3 LeftPlugPosition = Vector3.left * 32.0f;
        public static readonly Vector3 RightPlugPosition = Vector3.right * 32.0f;

        public bool IsValid { get; private set; }
        public DirectionType StartDirection { get; private set; }
        public int Count { get; private set; }
        public bool HasFront { get; private set; }
        public bool HasBack { get; private set; }
        public bool HasLeft { get; private set; }
        public bool HasRight { get; private set; }

        public bool TryGetNewRotation(uint seed, RotateType rotate, out Quaternion rotation)
        {
            var newDirection = GetNewDirection(seed, StartDirection, rotate);
            if (Count >= 4)
            {
                rotation = GetRotationOfDirection(newDirection);
                return true;
            }
            else
            {
                Logger.Info($"You are rotating {Count} side Geomorph, this could lead to level gen crash!");
                rotation = GetRotationOfDirection(newDirection);
                return true;
            }
        }

        public bool HasPlug(DirectionType direction)
        {
            if (!IsValid)
                return false;

            if (Count >= 4)
                return true;

            return direction switch
            {
                DirectionType.Forward => HasFront,
                DirectionType.Backward => HasBack,
                DirectionType.Left => HasLeft,
                DirectionType.Right => HasRight,
                _ => false
            };
        }

        public static LG_PlugInfo BuildPlugInfo(GameObject geoObject, Quaternion rotation)
        {
            var geomorph = geoObject.GetComponent<LG_Geomorph>();
            if (geomorph == null)
                return default;

            var plugs = geoObject.GetComponentsInChildren<LG_Plug>();
            if (plugs == null)
                return default;

            if (plugs.Length < 1)
                return default;

            var newPlugInfo = new LG_PlugInfo();

            foreach (var plug in plugs)
            {
                switch (plug.m_dir)
                {
                    case LG_PlugDir.Up:
                        newPlugInfo.HasBack = true;
                        newPlugInfo.Count++;
                        break;

                    case LG_PlugDir.Down:
                        newPlugInfo.HasFront = true;
                        newPlugInfo.Count++;
                        break;

                    case LG_PlugDir.Left:
                        newPlugInfo.HasLeft = true;
                        newPlugInfo.Count++;
                        break;

                    case LG_PlugDir.Right:
                        newPlugInfo.HasRight = true;
                        newPlugInfo.Count++;
                        break;
                }
            }
            newPlugInfo.IsValid = newPlugInfo.Count > 0;
            var eulerAngle = rotation.eulerAngles;
            var angle = Mathf.Round(eulerAngle.y);

            Logger.Verbose($"angle: {angle}");

            angle = Mathf.Repeat(angle, 360.0f);
            var index = Mathf.RoundToInt(angle / 90.0f);

            newPlugInfo.StartDirection = index switch
            {
                0 => DirectionType.Forward,
                1 => DirectionType.Right,
                2 => DirectionType.Backward,
                3 => DirectionType.Left,
                _ => DirectionType.Unknown
            };
            
            return newPlugInfo;
        }

        public static DirectionType GetNewDirection(uint seed, DirectionType direction, RotateType rotate)
        {
            if (direction == DirectionType.Unknown)
                return DirectionType.Unknown;

            if (rotate == RotateType.Towards_Random)
            {
                var nextIndex = new Random(seed.ToSeed()).Next(4);
                rotate = nextIndex switch
                {
                    0 => RotateType.Towards_Forward,
                    1 => RotateType.Towards_Backward,
                    2 => RotateType.Towards_Left,
                    3 => RotateType.Towards_Right,
                    _ => throw new IndexOutOfRangeException("Towards_Random: nextIndex was out of range?!")
                };
            }

            return rotate switch
            {
                RotateType.None => direction,
                RotateType.Flip => direction switch
                {
                    DirectionType.Forward => DirectionType.Backward,
                    DirectionType.Backward => DirectionType.Forward,
                    DirectionType.Left => DirectionType.Right,
                    DirectionType.Right => DirectionType.Left,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction))
                },
                RotateType.MoveTo_Left => direction switch
                {
                    DirectionType.Forward => DirectionType.Left,
                    DirectionType.Backward => DirectionType.Right,
                    DirectionType.Left => DirectionType.Backward,
                    DirectionType.Right => DirectionType.Forward,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction))
                },
                RotateType.MoveTo_Right => direction switch
                {
                    DirectionType.Forward => DirectionType.Right,
                    DirectionType.Backward => DirectionType.Left,
                    DirectionType.Left => DirectionType.Forward,
                    DirectionType.Right => DirectionType.Backward,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction))
                },
                RotateType.Towards_Forward => DirectionType.Forward,
                RotateType.Towards_Backward => DirectionType.Backward,
                RotateType.Towards_Left => DirectionType.Left,
                RotateType.Towards_Right => DirectionType.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        public static Quaternion GetRotationOfDirection(DirectionType direction)
        {
            return direction switch
            {
                DirectionType.Forward => NoRotation,
                DirectionType.Backward => BackwardRotation,
                DirectionType.Left => LeftRotation,
                DirectionType.Right => RightRotation,
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }
    }
}
