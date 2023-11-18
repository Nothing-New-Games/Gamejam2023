using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [MinValue(100), MaxValue(10000), Range(100, 10000)]
    public float XSensitivity = 400f;
    [MinValue(100), MaxValue(10000), Range(100, 10000)]
    public float YSensitivity = 400f;

    [MinValue(0), MaxValue(360), Range(0, 360)]
    public float YAxisClampDown = 90f;
    [MinValue(-360), MaxValue(0), Range(-360, 0)]
    public float YAxisClampUp = -90f;

    private Transform player;

    private float _CurrentYRot;
    private float _CurrentXRot;


    private void Awake()
    {
        player = GetComponentInParent<Player>().transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (Player.IsGamePaused) return;

        _CurrentXRot -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * YSensitivity;
        _CurrentYRot += Input.GetAxisRaw("Mouse X") * Time.deltaTime * XSensitivity;

        _CurrentXRot = Mathf.Clamp(_CurrentXRot, YAxisClampUp, YAxisClampDown);

        transform.rotation = Quaternion.Euler(_CurrentXRot, _CurrentYRot, 0);
        player.rotation = Quaternion.Euler(0, _CurrentYRot, 0);
    }
}
