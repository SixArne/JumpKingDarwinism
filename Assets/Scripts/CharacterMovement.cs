using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private CharacterController2D _characterController;

    void Start()
    {
        _characterController = GetComponent<CharacterController2D>();
        if (!_characterController)
        {
            throw new UnityException("Unable to find CharacterController2D");
        }
    }

    void Update()
    {
        // User inputs (obselete)
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool hasPressedJump = Input.GetKeyDown(KeyCode.Space);
        bool hasReleasedJump = Input.GetKeyUp(KeyCode.Space);

        // add movement
        _characterController.Move(horizontalInput);

        // handle jump
        if (hasPressedJump)
        {
            _characterController.StartJump();
        }
        else if (hasReleasedJump)
        {
            _characterController.EndJump();
        }
    }
}
