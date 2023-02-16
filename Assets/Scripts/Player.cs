using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : BoardEntity
{
    public override EntityType Types => EntityType.Player | EntityType.Solid;

    private const float _moveDuration = .12f;
    private bool _isMoving;

    private static readonly Queue<Vector3Int> _bufferedInputs = new Queue<Vector3Int>();

    private readonly Action<InputAction.CallbackContext> MoveLeft = (context) => Move(Vector3Int.left);
    private readonly Action<InputAction.CallbackContext> MoveRight = (context) => Move(Vector3Int.right);
    private readonly Action<InputAction.CallbackContext> MoveForward = (context) => Move(Vector3Int.forward);
    private readonly Action<InputAction.CallbackContext> MoveBack = (context) => Move(Vector3Int.back);
    private readonly Action<InputAction.CallbackContext> RegeneratePuzzle = (context) => Level.Instance.RegenerateLevel();

    private static void Move(Vector3Int direction) => _bufferedInputs.Enqueue(direction);

    private void OnEnable()
    {
        InputHandler.Actions.Gameplay.MoveLeft.performed += MoveLeft;
        InputHandler.Actions.Gameplay.MoveRight.performed += MoveRight;
        InputHandler.Actions.Gameplay.MoveUp.performed += MoveForward;
        InputHandler.Actions.Gameplay.MoveDown.performed += MoveBack;
        InputHandler.Actions.Gameplay.RegeneratePuzzle.performed += RegeneratePuzzle;
    }

    private void OnDisable()
    {
        InputHandler.Actions.Gameplay.MoveLeft.performed -= MoveLeft;
        InputHandler.Actions.Gameplay.MoveRight.performed -= MoveRight;
        InputHandler.Actions.Gameplay.MoveUp.performed -= MoveForward;
        InputHandler.Actions.Gameplay.MoveDown.performed -= MoveBack;
        InputHandler.Actions.Gameplay.RegeneratePuzzle.performed -= RegeneratePuzzle;
    }

    private void Update()
    {
        HandleBufferedInputs();
    }

    private void HandleBufferedInputs()
    {
        if (_bufferedInputs.Count > 0)
        {
            if (!_isMoving)
            {
                Vector3Int direction = _bufferedInputs.Dequeue();
                Vector3Int targetPosition = GridPosition + direction;
                if (ValidPositionForPlayer(targetPosition))
                {
                    MoveToPosition(targetPosition, _moveDuration / (_bufferedInputs.Count + 1));
                }
            }
        }
    }

    private bool ValidPositionForPlayer(Vector3Int targetPosition)
    {
        return Level.Board.EntityPresentAt(targetPosition + Vector3Int.down, EntityType.Solid) && 
            !Level.Board.EntityPresentAt(targetPosition, EntityType.Solid);
    }

    private void MoveToPosition(Vector3Int targetPosition, float moveDuration)
    {
        StartCoroutine(MoveToPositionCoroutine());
        IEnumerator MoveToPositionCoroutine()
        {
            _isMoving = true;
            Vector3Int startPosition = GridPosition;
            float time = 0f;
            while (time < moveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, time / moveDuration);
                time += Time.deltaTime;
                yield return null;
            }

            print(startPosition);
            Level.Board.MoveEntity(this, startPosition, targetPosition);
            _isMoving = false;
        }
    }
}
