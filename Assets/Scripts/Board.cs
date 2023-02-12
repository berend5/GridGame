using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    // where we hold all references to the entities on the board
    // the key is a Vector3Int so we can acces the entity by knowing its position
    // the value is a dictionary with an entity type as a key and a reference to the actual entity in the scene
    private readonly Dictionary<Vector3Int, Dictionary<EntityType, BoardEntity>> _entities = new Dictionary<Vector3Int, Dictionary<EntityType, BoardEntity>>();

    // returns the total number of entities on the board
    public int EntityCount
    {
        get
        {
            int count = 0;
            foreach (Dictionary<EntityType, BoardEntity> boardEntities in _entities.Values) 
            {
                count += boardEntities.Count;
            }

            return count;
        }
    }

    public List<Vector3Int> AllEntityPositions => _entities.Keys.ToList();

    private Vector3Int[] _directions = { Vector3Int.left, Vector3Int.right, Vector3Int.forward, Vector3Int.back };

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

    public void MoveEntity(BoardEntity entity, Vector3Int from, Vector3Int to)
    {
        Remove(entity, from);
        Add(entity, to);
    }

    // add an entity to the board
    public void Add(BoardEntity entity, Vector3Int forcePosition = default)
    {
        Vector3Int position = forcePosition == default ? entity.GridPosition : forcePosition;
        // if this position already contains a dictionary with entities,
        // we add the entity to the dictionary at this position
        if (_entities.TryGetValue(position, out var entitiesAtPosition))
        {
            entitiesAtPosition.Add(entity.Types, entity);
        }
        else
        {
            // otherwise, we create a new dictionary at the entities position and add the entity to it
            Dictionary<EntityType, BoardEntity> newEntitiesAtPosition = new Dictionary<EntityType, BoardEntity>();
            _entities.Add(position, newEntitiesAtPosition);
            newEntitiesAtPosition.Add(entity.Types, entity);
        }
    }

    // returns true if an entity was found, returns false if no entity was found
    public bool TryGetEntity(Vector3Int at, out BoardEntity entity, EntityType types)
    {
        entity = null;
        if (_entities.TryGetValue(at, out var entitiesAtPosition))
        {
            if (entitiesAtPosition.TryGetValue(types, out entity))
            {
                return true;
            }
        }
        
        return false;
    }

    // check if any entity of specified type is located at a position
    public bool EntityPresentAt(Vector3Int at, EntityType types)
    {
        // check if the dictionary at position we are looking at contains the type we are looking for
        if (_entities.TryGetValue(at, out var entitiesAtPosition))
        {
            if (entitiesAtPosition.ContainsKey(types))
            {
                return true;
            }
        }
        

        return false;
    }

    // remove any entity from the board
    public void Remove(BoardEntity entity, Vector3Int from = default) 
    {
        Vector3Int position = from == default ? entity.GridPosition : from;
        _entities[position].Remove(entity.Types);
    }

    public void Clear()
    {
        foreach (Dictionary<EntityType, BoardEntity> entities in _entities.Values)
        {
            foreach (BoardEntity entity in entities.Values)
            {
                entity.DestroyEntity();
            }
        }

        _entities.Clear();
    }
}
