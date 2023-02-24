using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace GridGame
{
    public class Pushable : NetworkBehaviour, IInteractable
    {
        private BoardEntity _entity;
        private bool _isInteracting;

        private void Start()
        {
            _entity = GetComponent<BoardEntity>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TryInteractServerRpc(Vector3Int inputDirection)
        {
            if (_isInteracting)
            {
                return;
            }

            Vector3Int startPosition = _entity.GridPosition;
            Vector3Int targetPosition = startPosition + inputDirection;
            if (Level.Board.TryGetEntity(targetPosition, out BoardEntity entity, TypeMask.Get(Flag.Interactable)))
            {   
                IInteractable interactable = entity.GetComponent<IInteractable>();
                interactable.TryInteractServerRpc(inputDirection);
            }

            if (Level.Board.IsEntityPresentAt(targetPosition, TypeMask.Get(Flag.Solid)))
            {
                return;
            }

            float duration = PlayerController.MoveDuration;
            EntityMove move = new EntityMove(startPosition, targetPosition, duration, _entity.LocalId);
            MoveClientRpc(move);
        }

        [ClientRpc]
        private void MoveClientRpc(EntityMove move)
        {
            Level.Board.MoveEntityData(move);
            StartCoroutine(MoveCoroutine());
            IEnumerator MoveCoroutine()
            {
                _isInteracting = true;
                StartCoroutine(Level.Board.MoveToPositionVisualCoroutine(move));
                yield return new WaitForSeconds(move.Duration);
                _isInteracting = false;
            }
        }
    }
}
