using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace GridGame
{
    public class Pushable : NetworkBehaviour, IPushable
    {
        private BoardEntity _entity;
        private bool _isInteracting;

        private void Start()
        {
            _entity = GetComponent<BoardEntity>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TryInteractServerRpc(Vector3Int pushDirection)
        {
            if (_isInteracting)
            {
                return;
            }

            Vector3Int startPosition = _entity.GridPosition;
            Vector3Int targetPosition = startPosition + pushDirection;
            if (Level.Board.TryGetEntity(targetPosition, out BoardEntity entity, TypeMask.Get(Flag.Interactable)))
            {   
                IPushable interactable = entity.GetComponent<IPushable>();
                interactable.TryInteractServerRpc(pushDirection);
            }

            if (Level.Board.IsEntityPresentAt(targetPosition, TypeMask.Get(Flag.Solid)))
            {
                return;
            }

            float duration = PlayerController.MoveDuration;
            MoveData move = new MoveData(startPosition, targetPosition, duration);
            MoveClientRpc(move);
        }

        [ClientRpc]
        private void MoveClientRpc(MoveData move)
        {
            StartCoroutine(MoveCoroutine());
            Level.Board.MoveEntityData(_entity, move);
            IEnumerator MoveCoroutine()
            {
                _isInteracting = true;
                StartCoroutine(Level.Board.MoveToPositionVisualCoroutine(_entity, move));
                yield return new WaitForSeconds(move.Duration);
                _isInteracting = false;
            }
        }
    }
}
