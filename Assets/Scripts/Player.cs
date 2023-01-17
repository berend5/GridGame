using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BoardEntity
{
    // the player is both a player type and a solid type
    public override EntityType Types => EntityType.Player | EntityType.Solid;

    private const float _moveDuration = .15f;
    private bool _isMoving;

    private void Update()
    {
        // gather direction based on inputs from this frame
        Vector3Int direction = GetDirectionIfInput();

        // if the direction we got back is not zero, we should try to move
        if (direction != Vector3Int.zero) 
        {
            if (!_isMoving)
            {
                // calculate target position by adding direction to current position
                Vector3Int targetPosition = GridPosition + direction;

                // we should check if the is any solid entity we can walk on 1 unit below our target position,
                // otherwise we are trying to move to a space that would make the player float

                // we also want to check if there is no solid entity at the target position, to prevent us from walking
                // through walls
                if (Level.Board.EntityPresentAt(targetPosition + Vector3Int.down, EntityType.Solid) ||
                    Level.Board.EntityPresentAt(targetPosition, EntityType.Solid))
                {
                    // finally we know we can move to the target position
                    MoveToPosition(targetPosition, _moveDuration);
                }
            }
        }
    }

    // simple move method
    private void MoveToPosition(Vector3Int targetPosition, float moveDuration)
    {
        StartCoroutine(MoveToPositionCoroutine());
        IEnumerator MoveToPositionCoroutine()
        {
            Level.Board.Remove(this); // we remove ourselves from the board at the start of moving
            _isMoving = true;
            Vector3Int startPosition = GridPosition;
            float time = 0f;
            while (time < moveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, time / moveDuration);
                time += Time.deltaTime;
                yield return null;
            }

            Level.Board.Add(this); // we add ourselves again when we stop moving, now with an updated position
            _isMoving = false;
        }
    }

    private Vector3Int GetDirectionIfInput()
    {
        // if any movement keys are pressed, return the corresponding direction
        if (Input.GetKey(KeyCode.W))
        {
            return Vector3Int.forward;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            return Vector3Int.left;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            return Vector3Int.back;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            return Vector3Int.right;
        }

        return Vector3Int.zero;
    }
}
