using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 10f;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _maxJumpValue = 20f;
    [SerializeField] private float _minJumpValue = 1f;
    [SerializeField] private PhysicsMaterial2D _bounceMat, _normalMat;
    [SerializeField] private float _maxJumpTime = 0.5f;
    [SerializeField] private float _distanceMultiplier = 2f;
    [SerializeField] private float _shortJumpMultiplier = 3f;
    
    private bool _isGrounded = false;
    private Rigidbody2D _rb;
    private bool _canJump = true;
    private float _jumpValue = 0f;
    private float _jumpTime = 0f;
    private CharacterMovement _cm;
    private SpriteRenderer _sr = null;
    private Animator _an;
    private MoveDirection _dir = MoveDirection.None;
    private float _shortJumpValue = 1f;
    private float _lastFrameVelocityX = 0f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (!_rb)
        {
            throw new UnityException("Couldn't find RigidbBody2D component");
        }

        _cm = GetComponent<CharacterMovement>();
        if (!_cm)
        {
            throw new UnityException("Couldn't find CharacterMovement component");
        }
        
        _sr = GetComponent<SpriteRenderer>();
        if (!_sr)
        {
            throw new UnityException("Couldn't find SpriteRenderer component");
        }

        _an = GetComponent<Animator>();
        if (!_an)
        {
            throw new UnityException("Animator not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Code for checking slanted areas
        RaycastHit2D hit = new RaycastHit2D();
        if (PlayerIsOnSlatedFloor(ref hit))
        {
            ApplySlipping(hit);
            return;
        }
        
        var input = Input.GetAxisRaw("Horizontal");
        
        // Animator checks
        UpdateAnimatorMoveDirection(input);
        UpdateAnimatorGrounded(input);
        
        // Ground collision check
        UpdateIsGrounded();

        // If player is grounded and didn't jump
        if (_jumpValue == 0f && _isGrounded)
        {
            _lastFrameVelocityX = _cm.Movement * _walkSpeed;
            _rb.velocity = new Vector2(_cm.Movement * _walkSpeed, _rb.velocity.y);
        }
        
        SwitchMaterialOnJump();

        // If player is holding jump wind up jump power
        if (_cm.IsHoldingJump && _isGrounded && _canJump)
        {
            // Set sprite animator parameter
            _an.SetBool("HasPressedJump", true);
            
            _jumpTime += Time.deltaTime;
            _jumpValue  = Mathf.Lerp(_minJumpValue, _maxJumpValue, _jumpTime / _maxJumpTime);
        }

        // When player tries to jump cancel any horizontal movement
        if (_cm.HasPressedJump && _isGrounded && _canJump)
        {
            _rb.velocity = new Vector2(0f, _rb.velocity.y);
        }

        // When jump is held maximum amount trigger jump
        if (_jumpTime >= _maxJumpTime && _isGrounded)
        {
            // Set sprite animator parameter
            _an.SetBool("HasPressedJump", false);
            
            // Get values and apply them
            float tempX = input * _walkSpeed * _distanceMultiplier;
            float tempY = _jumpValue;
            
            _rb.velocity = new Vector2(tempX, tempY);
            
            // Reset jump info once applied to rb
            Invoke("ResetJump", 0.2f);
        }
        
        // Premature release
        if (_cm.HasReleasedJump)
        {
            //MapJumpForce(input);
            
            // Set animator sprite info
            _an.SetBool("HasPressedJump", false);
            
            // If grounded apply jump
            if (_isGrounded)
            {
                float xDistance = input * _walkSpeed * _distanceMultiplier;
                float yDistance = _jumpValue;

                var displacement = new Vector2(xDistance, yDistance);
                Debug.Log(displacement);
                
                _rb.velocity = new Vector2(xDistance, yDistance);
                //_rb.AddForce(new Vector2(xDistance, 0), ForceMode2D.Impulse);
                
                _jumpValue = 0f;
            }

            // Reset data
            _shortJumpValue = 1f;
            _jumpTime = 0f;
            _canJump = true;
        }
    }

    void SwitchMaterialOnJump()
    {
        if (!_isGrounded)
        {
            _rb.sharedMaterial = _bounceMat;
        }
        else
        {
            _rb.sharedMaterial = _normalMat;
        }
    }
    
    void MapJumpForce(float input)
    {
        // small hop
        if (_jumpTime <= 0.30f && Mathf.Abs(input) > float.Epsilon)
        {
            _shortJumpValue = _shortJumpMultiplier;
            _jumpValue = _minJumpValue;
        }
    }

    void UpdateAnimatorMoveDirection(float input)
    {
        if (input < 0)
        {
            _sr.flipX = true;
            _dir = MoveDirection.Left;
        }
        else if (input == 0f)
        {
            if (_dir == MoveDirection.Left)
            {
                _sr.flipX = true;
            }
            else if (_dir == MoveDirection.Right)
            {
                _sr.flipX = false;
            }
        }
        else
        {
            _sr.flipX = false;
            _dir = MoveDirection.Right;
        }
    }

    void UpdateIsGrounded()
    {
        Vector3 pos = transform.position;

        _isGrounded = Physics2D.OverlapBox(
            new Vector2(pos.x - .05f, pos.y - .5f),
            new Vector2(.5f, .5f), 0f, _groundMask
        );

    }

    void UpdateAnimatorGrounded(float input)
    {
        if (input > 0 || input < -(float.Epsilon))
        {
            _an.SetBool("IsWalking", true);
        }
        else if (input <= float.Epsilon)
        {
            _an.SetBool("IsWalking", false);
        }

        if (!_isGrounded)
        {
            _an.SetBool("IsAirBorn", true);
        }
        else
        {
            _an.SetBool("IsAirBorn", false);
        }
    }

    bool PlayerIsOnSlatedFloor(ref RaycastHit2D hit)
    {
        bool isOnSlantedFloor = false;
        Vector3 spriteOffset = new Vector3(_sr.sprite.bounds.extents.x, 0f, 0f);

        // ray cast here to check if grounded
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position + spriteOffset, Vector2.down, 1, _groundMask);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position - spriteOffset, Vector2.down, 1, _groundMask);

        if (leftHit || rightHit)
        {
            // Check if horizontal
            float angleDistanceLeft = Vector2.Dot(leftHit.normal, Vector2.right);
            float angleDistanceRight = Vector2.Dot(leftHit.normal, Vector2.right);

            // If is on horizontal floor
            if (Mathf.Abs(angleDistanceLeft) <= float.Epsilon || Mathf.Abs(angleDistanceRight) <= float.Epsilon)
            {
                isOnSlantedFloor = false;
            }
            // If is on slanted floor
            else
            {
                isOnSlantedFloor = true;
                hit = leftHit;
            }
        }

        return isOnSlantedFloor;
    }

    void ApplySlipping(RaycastHit2D leftHit)
    {
        float direction = (leftHit.normal.x * -1) - (leftHit.normal.y * 0);
        Vector3 vectorInScreen = new Vector3(0, 0, 1);
        Vector3 normalVec3D = new Vector3(leftHit.normal.x, leftHit.normal.y, 0);

        Vector3 slopeDirection3D = Vector3.Cross(normalVec3D, vectorInScreen).normalized;
        float slopeIncline = Vector2.Dot(leftHit.normal, Vector2.down);
        Vector2 slopeDirection = Vector3.zero;

        if (direction < 0)
        {
            slopeDirection = - new Vector2(slopeDirection3D.x, slopeDirection3D.y);
        }
        else if (direction > 0)
        {
            slopeDirection = (new Vector2(slopeDirection3D.x, slopeDirection3D.y));
        }

        // Calculate slope velocity based on gravity
        var velocity = slopeDirection * Physics.gravity.magnitude * slopeIncline;
           
        // Apply velocity to rigidbody
        _rb.velocity = velocity;
    }

    void ResetJump()
    {
        _jumpValue = 0f;
        _jumpTime = 0f;
        _canJump = false;
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector2(pos.x - .05f, pos.y - .4f), new Vector2(.5f, .5f));
    }
}
