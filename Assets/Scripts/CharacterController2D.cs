using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private LayerMask _layerToCheckRaycast = 0;
    [SerializeField] private float _maxJumpTime = 2f;
    [SerializeField] private float _maxJumpForce = 10f;
    [SerializeField] private float _minJumpForce = 1f;
    [SerializeField] SpriteRenderer _DEBUG_SPRITE_RENDERER = null;

    // Component references
    private Rigidbody2D _rigidBody = null;
    private SpriteRenderer _spriteRenderer = null;
    
    private bool _isGrounded = false;
    private bool _isHoldingJump = false;
    private bool _heldJumpPreviousFrame = false;

    private bool _isOnSlantedFloor = false;
    private float _currentJumpTime = 0f;

    private Vector2 _velocity = Vector3.zero;
    private float _jumpForce = 0f;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        if (!_rigidBody)
        {
            throw new UnityException("Couldn't find RigidBody2D component");
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (!_spriteRenderer)
        {
            throw new UnityException("Couldn't find SpriteRenderer component");
        }
    }

    public void FixedUpdate()
    {
        HandleJumpCooldown();

        Vector3 spriteOffset = new Vector3(_spriteRenderer.sprite.bounds.extents.x, 0f, 0f);

        // ray cast here to check if grounded
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position + spriteOffset, Vector2.down, 1, _layerToCheckRaycast);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position - spriteOffset, Vector2.down, 1, _layerToCheckRaycast);

        // Now we need to check if ground is horizontal
        if (leftHit || rightHit)
        {
            // Check if horizontal
            float angleDistanceLeft = Vector2.Dot(leftHit.normal, Vector2.right);
            float angleDistanceRight = Vector2.Dot(leftHit.normal, Vector2.right);

            // If is on horizontal floor
            if (Mathf.Abs(angleDistanceLeft) <= float.Epsilon || Mathf.Abs(angleDistanceRight) <= float.Epsilon)
            {
                _isOnSlantedFloor = false;
                _isGrounded = true;
            }
            // If is on slanted floor
            else
            {
                _isGrounded = false;
                _isOnSlantedFloor = true;
            }
        }
        else
        {
            _isOnSlantedFloor = false;
            _isGrounded = false;
        }

        if (_isOnSlantedFloor)
        {
            float direction = (leftHit.normal.x * -1) - (leftHit.normal.y * 0);
            Vector3 vectorInScreen = new Vector3(0, 0, 1);
            Vector3 normalVec3D = new Vector3(leftHit.normal.x, leftHit.normal.y, 0);

            Vector3 slopeDirection3D = Vector3.Cross(normalVec3D, vectorInScreen).normalized;
            float slopeIncline = Vector2.Dot(leftHit.normal, Vector2.down);
            Vector2 slopeDirection = Vector3.zero;

            Debug.Log(string.Format("slope is: "));
            Debug.Log(slopeIncline);

            if (direction < 0)
            {
                slopeDirection = - new Vector2(slopeDirection3D.x, slopeDirection3D.y);
            }
            else if (direction > 0)
            {
                slopeDirection = (new Vector2(slopeDirection3D.x, slopeDirection3D.y));
            }

            // Calculate slope velocity based on gravity
            _velocity = slopeDirection * Physics.gravity.magnitude * slopeIncline;
           
            // Apply velocity to rigidbody
            _rigidBody.velocity = _velocity;
        }
        else
        {
            // Apply velocity to rigidbody
            _rigidBody.velocity = new Vector2(_velocity.x, _rigidBody.velocity.y + _velocity.y);
        }

        
        // Reset velocity
        _velocity = Vector3.zero;
    }

    public void HandleJumpCooldown()
    {
        // Currently holding jump
        if (_isHoldingJump)
        {
            _currentJumpTime += Time.deltaTime;
            _jumpForce = Mathf.Lerp(_minJumpForce, _maxJumpForce, _currentJumpTime / _maxJumpTime);

            if (_currentJumpTime >= _maxJumpTime)
            {
                // Force a jump due to time-out
                Jump();

                _currentJumpTime = 0;
                _isHoldingJump = false;
                _jumpForce = _maxJumpForce;

                return;
            }
        }
        // Released jump
        else if (!_isHoldingJump && _heldJumpPreviousFrame)
        {
            Jump();

            // Unset to avoid loop
            _heldJumpPreviousFrame = false;
        }
    }

    private void Jump()
    {
        Debug.Log(string.Format("Jumping with: %f", _jumpForce));
        _velocity.y = _jumpForce;
        _jumpForce = 0f;
    }

    public void Move(float horizontalInput)
    {
        if (_isGrounded)
        {
            float displacementWithSpeed = horizontalInput * _movementSpeed;
            _velocity = new Vector2(displacementWithSpeed, 0);
        }
    }

    public void StartJump()
    {
        // When player holds down jump key wind up jump power
        // if key held down for longer than max duration auto jump
        // if key released before max duration => jump

        if (_isGrounded)
        {
            _isHoldingJump = true;
            _heldJumpPreviousFrame = true;
        }
    }

    public void EndJump()
    {
        _isHoldingJump = false;
    }

    private void OnDrawGizmos()
    {
        Vector3 spriteOffset = new Vector3(_DEBUG_SPRITE_RENDERER.sprite.bounds.extents.x, 0f, 0f);

        Gizmos.DrawLine(transform.position + spriteOffset, transform.position + spriteOffset + Vector3.down);
        Gizmos.DrawLine(transform.position - spriteOffset, transform.position - spriteOffset + Vector3.down);
    }
}
