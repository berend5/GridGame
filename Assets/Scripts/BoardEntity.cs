using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GridGame
{
    public class BoardEntity : MonoBehaviour
    {
        public Vector3Int GridPosition => Vector3Int.RoundToInt(transform.position);
        public TypeMask TypeMask => TypeMask.Get(_flags);
        public int LocalId => _id;
        private int _id;

        [EnumMask]
        [SerializeField]
        private Flag _flags;

        private void OnEnable()
        {
            _id = GetInstanceID();
            Level.Board.Add(this);
        }

        private void OnDisable()
        {
            Level.Board.Remove(this);
        }

        public void DestroyEntity()
        {
            Destroy(gameObject);
        }
    }

    [Serializable]
    public struct TypeMask
    {
        public Flag Flags;

        private TypeMask(Flag flags)
        {
            Flags = flags;
        }

        public static TypeMask Get(params Flag[] flags)
        {
            Flag f = 0;
            foreach (var flag in flags)
            {
                f |= flag;
            }

            return new TypeMask(f);
        }

        // we consider 2 type masks equal if 1 of the bits is at the same position
        public static bool operator ==(TypeMask a, TypeMask b)
        {
            return (a.Flags & b.Flags) != 0;
        }

        public static bool operator !=(TypeMask a, TypeMask b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeMask other)
            {
                return (Flags & other.Flags) != 0;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Flags.GetHashCode();
        }

        public override string ToString()
        {
            return Flags.ToString();
        }
    }

    [Flags]
    public enum Flag : uint
    {
        Solid =             1 << 0,
        Player =            1 << 1,
        Interactable =      1 << 2
    }

    public class EnumMaskAttribute : Attribute
    {

    }

    // 0x00000002
    // 0x00000004
    // 0x00000008
    // 0x00000010
    // 0x00000020
    // 0x00000040
    // 0x00000080
    // 0x00000100
    // 0x00000200
    // 0x00000400
    // 0x00000800
    // 0x00001000
    // 0x00002000
    // 0x00004000

    public interface IInteractable
    {
        public void TryInteractServerRpc(Vector3Int inputDirection);
    }
}
