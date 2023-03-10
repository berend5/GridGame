using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame
{
    public sealed class PlayerController : NetworkBehaviour
    {
        public static float MoveDuration = .12f;

        private BoardEntity _entity;
        private bool _lockMovement;

        private readonly Queue<Vector3Int> _bufferedInputs = new Queue<Vector3Int>();

        private Action<InputAction.CallbackContext> MoveLeft;
        private Action<InputAction.CallbackContext> MoveRight;
        private Action<InputAction.CallbackContext> MoveForward;
        private Action<InputAction.CallbackContext> MoveBack;

        private void Move(Vector3Int direction) => _bufferedInputs.Enqueue(direction);

        private IEnumerator Start()
        {
            _entity = GetComponent<BoardEntity>();
            yield return new WaitForSeconds(.2f); // isowner is sometimes not initialized so we wait a bit...
            
            if (IsOwner)
            {
                MoveLeft = (context) => Move(Vector3Int.left);
                MoveRight = (context) => Move(Vector3Int.right);
                MoveForward = (context) => Move(Vector3Int.forward);
                MoveBack = (context) => Move(Vector3Int.back);

                InputHandler.Actions.Gameplay.MoveLeft.performed += MoveLeft;
                InputHandler.Actions.Gameplay.MoveRight.performed += MoveRight;
                InputHandler.Actions.Gameplay.MoveUp.performed += MoveForward;
                InputHandler.Actions.Gameplay.MoveDown.performed += MoveBack;
            }
        }

        private void OnDisable()
        {
            if (IsOwner)
            {
                InputHandler.Actions.Gameplay.MoveLeft.performed -= MoveLeft;
                InputHandler.Actions.Gameplay.MoveRight.performed -= MoveRight;
                InputHandler.Actions.Gameplay.MoveUp.performed -= MoveForward;
                InputHandler.Actions.Gameplay.MoveDown.performed -= MoveBack;
            }
        }

        private void Update()
        {
            HandleBufferedInputs();
        }

        private void HandleBufferedInputs()
        {
            if (_bufferedInputs.Count > 0)
            {
                if (!_lockMovement)
                {
                    Vector3Int direction = _bufferedInputs.Dequeue();
                    Vector3Int startPosition = _entity.GridPosition;
                    Vector3Int targetPosition = startPosition + direction;
                    if (ValidPositionForPlayer(targetPosition, direction))
                    {
                        float duration = MoveDuration / (_bufferedInputs.Count + 1);
                        MoveData move = new MoveData(startPosition, targetPosition, duration);
                        MoveToPositionServerRpc(move);
                        _lockMovement = true;
                    }
                }
            }
        }

        private bool ValidPositionForPlayer(Vector3Int targetPosition, Vector3Int inputDirection)
        {
            if (Level.Board.TryGetEntity(targetPosition, out BoardEntity entity, TypeMask.Get(Flag.Interactable)))
            {
                entity.GetComponent<IPushable>().TryInteractServerRpc(inputDirection);
            }

            return Level.Board.IsEntityPresentAt(targetPosition + Vector3Int.down, TypeMask.Get(Flag.Solid)) &&
                !Level.Board.IsEntityPresentAt(targetPosition, TypeMask.Get(Flag.Solid));
        }

        [ServerRpc(RequireOwnership = false)]
        private void MoveToPositionServerRpc(MoveData move)
        {
            MoveToPositionClientRpc(move);
        }

        [ClientRpc]
        private void MoveToPositionClientRpc(MoveData move)
        {
            StartCoroutine(Level.Board.MoveToPositionVisualCoroutine(_entity, move));
            Level.Board.MoveEntityData(_entity, move);
            StartCoroutine(WaitCoroutine());
            IEnumerator WaitCoroutine()
            {
                yield return new WaitForSeconds(move.Duration);
                _lockMovement = false;
            }
        }
    }
}
