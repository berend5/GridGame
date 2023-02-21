using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridGame
{
    public class Pushable : MonoBehaviour, IInteractable
    {
        private BoardEntity _entity;
        private bool _isInteracting;

        private void Start()
        {
            _entity = GetComponent<BoardEntity>();
        }

        public bool TryInteract(Vector3Int inputDirection)
        {
            if (_isInteracting)
            {
                return false;
            }

            Vector3Int startPosition = _entity.GridPosition;
            Vector3Int targetPosition = startPosition + inputDirection;
            
            if (Level.Board.TryGetEntity(targetPosition, out BoardEntity entity, TypeMask.Get(Flag.Interactable)))
            {   
                IInteractable interactable = entity.GetComponent<IInteractable>();
                bool interacted = interactable.TryInteract(inputDirection);
                print(interacted);
                if (!interacted)
                {
                    return false;
                }
            }

            if (Level.Board.IsEntityPresentAt(targetPosition, TypeMask.Get(Flag.Solid)))
            {
                return false;
            }

            StartCoroutine(MoveCoroutine());
            return true;

            IEnumerator MoveCoroutine()
            {
                _isInteracting = true;
                float duration = PlayerController.MoveDuration;
                EntityMove move = new EntityMove(startPosition, targetPosition, duration);
                StartCoroutine(Level.Board.MoveToPositionVisualCoroutine(_entity, move));
                Level.Board.MoveEntityData(_entity, move.StartPosition, move.TargetPosition);
                yield return new WaitForSeconds(duration);
                _isInteracting = false;
            }
        }
    }
}
