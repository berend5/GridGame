using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : BoardEntity
{
    public override EntityType Types => EntityType.Player | EntityType.Solid;

    //Player movement Inputs
    private readonly Action<InputAction.CallbackContext> MoveLeft = (context) => Move(Vector3Int.left);
    private readonly Action<InputAction.CallbackContext> MoveRight = (context) => Move(Vector3Int.right);
    private readonly Action<InputAction.CallbackContext> MoveForward = (context) => Move(Vector3Int.forward);
    private readonly Action<InputAction.CallbackContext> MoveBack = (context) => Move(Vector3Int.back);

    //Level Inputs
    private readonly Action<InputAction.CallbackContext> RegeneratePuzzle = (context) => Level.Instance.RegenerateLevel();

    //Move blocks Inputs
    private readonly Action<InputAction.CallbackContext> MoveBlockLeft = (context) => MoveBlock(Vector3Int.left);
    private readonly Action<InputAction.CallbackContext> MoveBlockRight = (context) => MoveBlock(Vector3Int.right);
    private readonly Action<InputAction.CallbackContext> MoveBlockForward = (context) => MoveBlock(Vector3Int.forward);
    private readonly Action<InputAction.CallbackContext> MoveBlockBack = (context) => MoveBlock(Vector3Int.back);

    //Player Movement variables
    private const float _moveDuration = .12f;
    private bool _isMoving;
    private static readonly Queue<Vector3Int> _bufferedMovementInputs = new Queue<Vector3Int>();
    private static void Move(Vector3Int direction) => _bufferedMovementInputs.Enqueue(direction);

    //Block move variables
    private const float _moveBlockDuration = 0.05f;
    private static Vector3Int _moveBlockDirection =  Vector3Int.zero;
    private static void MoveBlock(Vector3Int direction) => _moveBlockDirection = direction;

    protected override void Start()
    {
        base.Start();
        InputHandler.Actions.Gameplay.MoveLeft.performed += MoveLeft;
        InputHandler.Actions.Gameplay.MoveRight.performed += MoveRight;
        InputHandler.Actions.Gameplay.MoveUp.performed += MoveForward;
        InputHandler.Actions.Gameplay.MoveDown.performed += MoveBack;
        InputHandler.Actions.Gameplay.RegeneratePuzzle.performed += RegeneratePuzzle;

        InputHandler.Actions.Gameplay.MoveBlockLeft.performed += MoveBlockLeft;
        InputHandler.Actions.Gameplay.MoveBlockRight.performed += MoveBlockRight;
        InputHandler.Actions.Gameplay.MoveBlockUp.performed += MoveBlockForward;
        InputHandler.Actions.Gameplay.MoveBlockDown.performed += MoveBlockBack;
    }

    private void OnDisable()
    {
        InputHandler.Actions.Gameplay.MoveLeft.performed -= MoveLeft;
        InputHandler.Actions.Gameplay.MoveRight.performed -= MoveRight;
        InputHandler.Actions.Gameplay.MoveUp.performed -= MoveForward;
        InputHandler.Actions.Gameplay.MoveDown.performed -= MoveBack;
        InputHandler.Actions.Gameplay.RegeneratePuzzle.performed -= RegeneratePuzzle;

        InputHandler.Actions.Gameplay.MoveBlockLeft.performed -= MoveBlockLeft;
        InputHandler.Actions.Gameplay.MoveBlockRight.performed -= MoveBlockRight;
        InputHandler.Actions.Gameplay.MoveBlockUp.performed -= MoveBlockForward;
        InputHandler.Actions.Gameplay.MoveBlockDown.performed -= MoveBlockBack;
    }

    private void Update()
    {
        HandleBufferedMovementInputs();
        MoveBlockInDirection();
    }

    private void MoveBlockInDirection()
    {
        //Is moving gebruiken is beetje vies want als je terwijl t moven
        if(_moveBlockDirection != Vector3Int.zero && !_isMoving)
        {
                //What is opposite direction?
                Vector3Int targetDirection = GridPosition + _moveBlockDirection * -1;
                //Is there any space behind player?
                if (ValidPositionForBlock(targetDirection, out var targetPosition) && Level.Board.TryGetEntity(GridPosition + _moveBlockDirection, out var entityToMove, EntityType.Solid) && entityToMove.isInteractable)
                {
                    MoveToPosition(targetPosition, _moveBlockDuration, entityToMove);
                }
                _moveBlockDirection = Vector3Int.zero;
        }
    }

    private void HandleBufferedMovementInputs()
    {
        if (_bufferedMovementInputs.Count > 0)
        {
            if (!_isMoving)
            {
                Vector3Int direction = _bufferedMovementInputs.Dequeue();
                Vector3Int targetPosition = GridPosition + direction;
                if (ValidPositionForPlayer(targetPosition))
                {
                    MoveToPosition(targetPosition, _moveDuration / (_bufferedMovementInputs.Count + 1), this);
                }
            }
        }
    }

    private bool ValidPositionForPlayer(Vector3Int targetPosition)
    {
        bool positionIsValid = Level.Board.EntityPresentAt(targetPosition + Vector3Int.down, EntityType.Solid) &&
            !Level.Board.EntityPresentAt(targetPosition, EntityType.Solid);
        return positionIsValid;
    }    
    
    private bool ValidPositionForBlock(Vector3Int targetDirection, out Vector3Int targetPosition)
    {
        bool positionIsValid = !Level.Board.EntityPresentAt(targetDirection, EntityType.Solid);
        if(Level.Board.EntityPresentAt(targetDirection + Vector3Int.down, EntityType.Solid))
        {
            targetPosition = targetDirection;
        } else
        {
            targetPosition = targetDirection + Vector3Int.down;
        }

        return positionIsValid;
    }

    private void MoveToPosition(Vector3Int targetPosition, float moveDuration, BoardEntity entityToMove)
    {
        StartCoroutine(MoveToPositionCoroutine());
        IEnumerator MoveToPositionCoroutine()
        {
            _isMoving = true;
            Vector3Int startPosition = entityToMove.GridPosition;
            float time = 0f;
            while (entityToMove.transform.position != targetPosition)
            {
                entityToMove.transform.position = Vector3.Lerp(startPosition, targetPosition, time / moveDuration);
                time += Time.deltaTime;
                yield return null;
            }
            //entityToMove.transform.position = targetPosition;
            Level.Board.MoveEntity(entityToMove, startPosition, targetPosition);
            _isMoving = false;
        }
    }
}
