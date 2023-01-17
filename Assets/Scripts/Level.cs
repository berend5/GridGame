using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }

    public static Board Board { get; private set; }

    private void Awake()
    {
        Instance = this;
        Board = new Board();
    }
}
