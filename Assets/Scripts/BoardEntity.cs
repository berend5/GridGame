using System;
using System.Collections;
using UnityEngine;

// the board entity class is the base class for any entity in the scene that exits on the board

// all board entities know their current position and can be of multiple types at once
public class BoardEntity : MonoBehaviour
{
    // returns the rounded transform.position of this entity
    public Vector3Int GridPosition => Vector3Int.RoundToInt(transform.position);

    // the type of this entity, we can use the | operator to assign multiple types to the entity
    public virtual EntityType Types => EntityType.Solid;

    // on start, we add this entity to the board
    protected virtual void Start()
    {
        Level.Board.Add(this);
    }

    public void DestroyEntity()
    {
        Destroy(gameObject);
    }
}

// the types an entity can be
public enum EntityType
{
    Solid,
    Player,
    Interactable
}
