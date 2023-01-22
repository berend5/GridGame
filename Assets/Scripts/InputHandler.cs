using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static PlayerActions Actions;

    private void OnEnable()
    {
        Actions = new PlayerActions();
        Actions.Enable();
    }

    private void OnDisable()
    {
        Actions.Disable();
    }
}
