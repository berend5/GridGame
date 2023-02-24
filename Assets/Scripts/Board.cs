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
        private readonly Dictionary<Vector3Int, List<BoardEntity>> _entitiesByPosition = new Dictionary<Vector3Int, List<BoardEntity>>();
        private readonly Dictionary<int, BoardEntity> _entitiesById = new Dictionary<int, BoardEntity>();

        public int EntityCount
        {
            get
            {
                int count = 0;
                foreach (var boardEntities in _entitiesByPosition.Values)
                {
                    count += boardEntities.Count;
                }

                return count;
            }
        }

        public List<Vector3Int> AllEntityPositions => _entitiesByPosition.Keys.ToList();

        private readonly Vector3Int[] _directions = { Vector3Int.left, Vector3Int.right, Vector3Int.forward, Vector3Int.back };

        public IEnumerator MoveToPositionVisualCoroutine(EntityMove move)
        {
            float time = 0f;
            BoardEntity entity = _entitiesById[move.EntityId];
            while (time < move.Duration)
            {
                entity.transform.position = Vector3.Lerp(move.StartPosition, move.TargetPosition, time / move.Duration);
                time += Time.deltaTime;
                yield return null;
            }

            entity.transform.position = move.TargetPosition;
        }

        public List<Vector3Int> AllEmptyPositionsNextTo(Vector3Int from)
        {
            List<Vector3Int> nextTo = new List<Vector3Int>();
            for (int i = 0; i < _directions.Length; i++)
            {
                Vector3Int toCheck = from + _directions[i];
                if (!_entitiesByPosition.ContainsKey(toCheck))
                {
                    nextTo.Add(toCheck);
                }
            }

            return nextTo;
        }

        public void MoveEntityData(EntityMove move)
        {
            BoardEntity entity = _entitiesById[move.EntityId];
            Remove(entity, move.StartPosition);
            Add(entity, move.TargetPosition);
        }

        public void Add(BoardEntity entity, Vector3Int? forcePosition = null) // need to use nullable since default defaults to (0, 0, 0)
        {
            // entities by position
            Vector3Int position = forcePosition == null ? entity.GridPosition : (Vector3Int) forcePosition;
            if (_entitiesByPosition.TryGetValue(position, out List<BoardEntity> entitiesAtPosition))
            {
                entitiesAtPosition.Add(entity);
            }
            else
            {
                var newEntitiesAtPosition = new List<BoardEntity>() { entity };
                _entitiesByPosition.Add(position, newEntitiesAtPosition);
            }

            // entities by id
            if (!_entitiesById.ContainsKey(entity.LocalId))
            {
                _entitiesById.Add(entity.LocalId, entity);
            }
        }

        public void Remove(BoardEntity entity, Vector3Int? from = null) // need to use nullable since default defaults to (0, 0, 0)
        {
            Vector3Int position = from == null ? entity.GridPosition : (Vector3Int) from;
            _entitiesByPosition[position].Remove(entity);
            _entitiesById.Remove(entity.LocalId);
        }

        // <summary> not a very optimized method, loops through all entities at position </summary>
        public bool TryGetEntity(Vector3Int at, out BoardEntity entity, TypeMask typeMask)
        {
            entity = null;
            if (_entitiesByPosition.TryGetValue(at, out var entitiesAtPosition))
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
            if (_entitiesByPosition.TryGetValue(at, out var entitiesAtPosition))
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
                return entity == null ? false : true;
            }

            return false;
        }

        public void Clear()
        {
            foreach (var entities in _entitiesByPosition.Values)
            {
                foreach (BoardEntity entity in entities)
                {
                    entity.DestroyEntity();
                }
            }

            _entitiesByPosition.Clear();
        }
    }

    public struct EntityMove : INetworkSerializable
    {
        public Vector3Int StartPosition;
        public Vector3Int TargetPosition;
        public float Duration;
        public int EntityId;

        public EntityMove(Vector3Int startPosition, Vector3Int targetPosition, float duration, int entityId)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Duration = duration;
            EntityId = entityId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref StartPosition);
            serializer.SerializeValue(ref TargetPosition);
            serializer.SerializeValue(ref Duration);
            serializer.SerializeValue(ref EntityId);
        }
    }
}
