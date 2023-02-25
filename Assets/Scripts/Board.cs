using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

namespace GridGame
{
    public class Board
    {
        private readonly Dictionary<Vector3Int, List<BoardEntity>> _entities = new Dictionary<Vector3Int, List<BoardEntity>>();

        public int EntityCount
        {
            get
            {
                int count = 0;
                foreach (var boardEntities in _entities.Values)
                {
                    count += boardEntities.Count;
                }

                return count;
            }
        }

        public List<Vector3Int> AllEntityPositions => _entities.Keys.ToList();

        private readonly Vector3Int[] _directions = { Vector3Int.left, Vector3Int.right, Vector3Int.forward, Vector3Int.back };
        public IEnumerator MoveToPositionVisualCoroutine(BoardEntity entity, MoveData move)
        {
            float time = 0f;
            float duration = move.Duration;
            Vector3Int startPosition = move.StartPosition;
            Vector3Int targetPosition = move.TargetPosition;
            while (time < move.Duration)
            {
                entity.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            entity.transform.position = targetPosition;
        }

        public List<Vector3Int> AllEmptyPositionsNextTo(Vector3Int from)
        {
            List<Vector3Int> nextTo = new List<Vector3Int>();
            for (int i = 0; i < _directions.Length; i++)
            {
                Vector3Int toCheck = from + _directions[i];
                if (!_entities.ContainsKey(toCheck))
                {
                    nextTo.Add(toCheck);
                }
            }

            return nextTo;
        }

        public void MoveEntityData(BoardEntity entity, MoveData move)
        {
            Remove(entity, move.StartPosition);
            Add(entity, move.TargetPosition);
        }

        public void Add(BoardEntity entity, Vector3Int at) // need to use nullable since default defaults to (0, 0, 0)
        {
            // entities by position
            //Vector3Int position = forcePosition == null ? entity.GridPosition : (Vector3Int) forcePosition;
            if (_entities.TryGetValue(at, out List<BoardEntity> entitiesAtPosition))
            {
                entitiesAtPosition.Add(entity);
            }
            else
            {
                var newEntitiesAtPosition = new List<BoardEntity>() { entity };
                _entities.Add(at, newEntitiesAtPosition);
            }
        }

        // remove assumes entity exists on the board. should remove chech for null or should the caller check?
        public void Remove(BoardEntity entity, Vector3Int from) // need to use nullable since default defaults to (0, 0, 0)
        {
            _entities[from].Remove(entity);
        }

        // not a very optimized method, loops through all entities at position
        public bool TryGetEntity(Vector3Int at, out BoardEntity entity, TypeMask typeMask)
        {
            entity = null;
            if (_entities.TryGetValue(at, out var entitiesAtPosition))
            {
                entity = GetEntityFromList(entitiesAtPosition, typeMask);
                return entity != null;
            }

            return false;
        }

        private BoardEntity GetEntityFromList(List<BoardEntity> entities, TypeMask typeMask)
        {
            foreach (var entity in entities)
            {
                if (entity.TypeMask == typeMask)
                {
                    return entity;
                }
            }

            return null;
        }

        public bool IsEntityPresentAt(Vector3Int at, TypeMask typeMask = default)
        {
            if (_entities.TryGetValue(at, out var entitiesAtPosition))
            {
                if (typeMask == default)
                {
                    if (entitiesAtPosition.Count > 0)
                    {
                        return true;
                    }

                    return false;
                }

                BoardEntity entity = GetEntityFromList(entitiesAtPosition, typeMask);
                return entity != null;
            }

            return false;
        }

        public void Clear()
        {
            foreach (var entities in _entities.Values)
            {
                foreach (BoardEntity entity in entities)
                {
                    entity.DestroyEntity();
                }
            }

            _entities.Clear();
        }
    }

    public struct MoveData : INetworkSerializable
    {
        public Vector3Int StartPosition;
        public Vector3Int TargetPosition;
        public float Duration;

        public MoveData(Vector3Int startPosition, Vector3Int targetPosition, float duration)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Duration = duration;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref StartPosition);
            serializer.SerializeValue(ref TargetPosition);
            serializer.SerializeValue(ref Duration);
        }
    }
}
