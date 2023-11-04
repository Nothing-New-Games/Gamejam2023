using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public Vector3 Offset;
    void Update()
    {
        transform.position = Input.mousePosition + Offset;
    }
}
