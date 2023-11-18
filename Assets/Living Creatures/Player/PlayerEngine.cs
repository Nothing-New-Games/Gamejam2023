using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEngine
{
    public PlayerEngine(Transform transform)
    {
        _PlayerTransform = transform;
        _RB = transform.GetComponent<Rigidbody>();
    }

    private Transform _PlayerTransform;
    private Rigidbody _RB;
    private float HorizontalInput;
    private float VerticalInput;

    public void HandleMovement(float movementSpeed)
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");

        Vector3 directionalForce = _PlayerTransform.forward * movementSpeed * VerticalInput + _PlayerTransform.right * movementSpeed * HorizontalInput;
        _RB.AddForce(directionalForce, ForceMode.Force);

        if (Player.GetPlayerInstance.IsGrounded)
            _RB.drag = Player.GetPlayerInstance.MaxGroundVelocity;


        if (_RB.velocity.magnitude > Player.GetPlayerInstance.MaxAirbornVelocity && !Player.GetPlayerInstance.IsGrounded)
            _RB.velocity = Vector3.ClampMagnitude(_RB.velocity, Player.GetPlayerInstance.MaxAirbornVelocity);
        else if (_RB.velocity.magnitude > Player.GetPlayerInstance.MaxGroundVelocity)
            _RB.velocity = Vector3.ClampMagnitude(_RB.velocity, Player.GetPlayerInstance.MaxGroundVelocity);
    }

    public void HandleJump(float jumpSpeed)
    {
        Vector3 jumpForce = _PlayerTransform.up * jumpSpeed;
        _RB.AddForce(jumpForce, ForceMode.Force);
    }
}
