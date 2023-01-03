using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private float _movement = 0f;
    private bool _isHoldingJump = false;
    private bool _hasReleasedJump = false;
    private bool _hasPressedJump = false;

    public bool HasPressedJump => _hasPressedJump;
    public bool IsHoldingJump => _isHoldingJump;
    public bool HasReleasedJump => _hasReleasedJump;
    public float Movement => _movement;

    void Update()
    {
        // User inputs TODO: Delete later
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool hasPressedJump = Input.GetKeyDown(KeyCode.Space);
        bool isHoldingJumping = Input.GetKey(KeyCode.Space);
        bool hasReleasedJump = Input.GetKeyUp(KeyCode.Space);

        _isHoldingJump = isHoldingJumping;
        _hasPressedJump = hasPressedJump;
        _hasReleasedJump = hasReleasedJump;
        _movement = horizontalInput;
    }
}
