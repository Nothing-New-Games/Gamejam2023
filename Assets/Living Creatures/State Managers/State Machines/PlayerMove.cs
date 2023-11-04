using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : State
{
    //Simply debug.log when user presses one of the movement keys.
    public override void Initialize()
    {
        _PossibleTransitions = new()
        {

        };
    }

    public override void OnUpdateState()
    {
        if (Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0)
        {
            Debug.Log("The user is pressing a movement key!");
        }
    }
}
