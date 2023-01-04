
#define _USER_INPUT

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class CharacterMovement : MonoBehaviour
{
    private float _movement = 0f;
    private bool _isHoldingJump = false;
    private bool _hasReleasedJump = false;
    private bool _hasPressedJump = false;

    public bool HasPressedJump
    {
        get => _hasPressedJump;
        set => _hasPressedJump = value;
    }

    public bool IsHoldingJump
    {
        get => _isHoldingJump;
        set => _isHoldingJump = value;
    }

    public bool HasReleasedJump
    {
        get => _hasReleasedJump;
        set => _hasReleasedJump = value;
    }

    public float Movement
    {
        get => _movement;
        set => _movement = value;
    }

    void Update()
    {
        #if _USER_INPUT
            _hasPressedJump = Input.GetKeyDown("space");
            _isHoldingJump = Input.GetKey("space");
            _hasReleasedJump = Input.GetKeyUp("space");
            _movement = Input.GetAxisRaw("Horizontal");
        #endif
        
    }
}
