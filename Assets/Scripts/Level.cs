using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }
    public static Board Board { get; private set; }

    [SerializeField] private Material _material1, _material2;
    [SerializeField] private BoardEntity _groundEntityPrefab;
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private int _iterations = 30;
    [SerializeField] private int _maxLevelSize = 6;

    private void Awake()
    {
        Instance = this;
        Board = new Board();
        //EnableLevelGameObjects();
        //RegenerateLevel();
    }

    //Zeer vieze functie groetjes Dion : )
    public void EnableLevelGameObjects()
    {
        foreach (Transform levelEntityTransform in transform.GetComponentInChildren<Transform>())
        {
            levelEntityTransform.gameObject.SetActive(true);
        }
    }

    public void RegenerateLevel()
    {
        Board.Clear();
        BoardEntity startEntity = Instantiate(_groundEntityPrefab, Vector3.zero, Quaternion.identity, transform);
        SetColor(startEntity);

        for (int i = 0; i < _iterations; i++)
        {
            List<Vector3Int> availablePositions = new List<Vector3Int>();
            foreach (Vector3Int position in Board.AllEntityPositions)
            {
                List<Vector3Int> freePositions = Board.AllEmptyPositionsNextTo(position);

                // remove positions out of level bounds
                List<Vector3Int> positionsToRemove = new List<Vector3Int>();
                for (int j = 0; j < freePositions.Count; j++)
                {
                    if (!IsWithinLevelBounds(freePositions[j]))
                    {
                        positionsToRemove.Add(freePositions[j]);
                    }
                }

                foreach(Vector3Int positionToRemove in positionsToRemove)
                {
                    freePositions.Remove(positionToRemove);
                }

                // add positions to available position list
                if (freePositions.Count > 0)
                {
                    availablePositions.Add(freePositions[Random.Range(0, freePositions.Count - 1)]);
                }
            }

            // pick a random position and instantiate the entity
            if (availablePositions.Count > 0)
            {
                Vector3Int chosenPosition = availablePositions[Random.Range(0, availablePositions.Count - 1)];
                BoardEntity entity = Instantiate(_groundEntityPrefab, chosenPosition, Quaternion.identity, transform);
                SetColor(entity);
            }
        }

        // spawn a player on a random position
        Vector3Int randomStartPosition = Board.AllEntityPositions[Random.Range(0, Board.AllEntityPositions.Count - 1)];
        Instantiate(_playerPrefab, randomStartPosition + new Vector3Int(0, 1, 0), Quaternion.identity, transform);
    }

    private bool IsWithinLevelBounds(Vector3Int position)
    {
        int max = (_maxLevelSize == 1) ? 1 : _maxLevelSize / 2;
        return Mathf.Abs(position.x) <= max && Mathf.Abs(position.z) <= max;
    }

    private void SetColor(BoardEntity entity)
    {
        int xAbs = Mathf.Abs(entity.GridPosition.x);
        int zAbs = Mathf.Abs(entity.GridPosition.z);
        Material material = ((xAbs % 2 == 0 && zAbs % 2 == 0) || (xAbs % 2 == 1 && zAbs % 2 == 1)) ? _material1 : _material2;
        entity.GetComponent<Renderer>().material = material;
    }

    // TODO: move this out to some debug class at some point
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireCube(transform.position, new Vector3(_maxLevelSize + 1f, _maxLevelSize + 1f, _maxLevelSize + 1f));
    }
#endif
}
