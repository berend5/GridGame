using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }
    public static Board Board { get; private set; }

    [SerializeField] private Material _material1, _material2;
    [SerializeField] private BoardEntity _groundEntityPrefab;
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private int _iterations = 30;

    private void Awake()
    {
        Instance = this;
        Board = new Board();
        RegenerateBoard();
    }

    public void RegenerateBoard()
    {
        Board.Clear();

        BoardEntity startEntity = Instantiate(_groundEntityPrefab, Vector3.zero, Quaternion.identity, transform);
        SetColor(startEntity);

        for (int i = 0; i < _iterations; i++)
        {
            List<Vector3Int> availablePositions = new List<Vector3Int>();
            foreach (Vector3Int position in Board.AllEntityPositions)
            {
                List<Vector3Int> freePositions = Board.AllFreePositionsNextTo(position);
                if (freePositions.Count > 0)
                {
                    availablePositions.Add(freePositions[Random.Range(0, freePositions.Count - 1)]);
                }
            }

            Vector3Int chosenPosition = availablePositions[Random.Range(0, availablePositions.Count - 1)];
            BoardEntity entity = Instantiate(_groundEntityPrefab, chosenPosition, Quaternion.identity, transform);
            SetColor(entity);
        }

        Vector3Int randomStartPosition = Board.AllEntityPositions[Random.Range(0, Board.AllEntityPositions.Count - 1)];
        Instantiate(_playerPrefab, randomStartPosition + new Vector3Int(0, 1, 0), Quaternion.identity, transform);
    }

    private void SetColor(BoardEntity entity)
    {
        int xAbs = Mathf.Abs(entity.GridPosition.x);
        int zAbs = Mathf.Abs(entity.GridPosition.z);
        Material material = ((xAbs % 2 == 0 && zAbs % 2 == 0) || (xAbs % 2 == 1 && zAbs % 2 == 1)) ? _material1 : _material2;
        entity.GetComponent<Renderer>().material = material;
    }
}
