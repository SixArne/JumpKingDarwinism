

#define _USER_INPUT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerInfo
{
    public DarwinBrain brain;
    public Vector3 position;
    public bool isGrounded;
    public float bestHeightReached;
    public int bestHeightReachedAtStepNr;
    public int brainActionNr;
    public MoveDirection moveDir;
    public bool isWaitingToStartAction;
    public bool startedAction;
}

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
    private MoveDirection _dir = MoveDirection.Left;
    private float _shortJumpValue = 1f;
    private float _lastFrameVelocityX = 0f;
    private Vector2 _currentFrameVelocity;
  
    
    // AI
    private DarwinBrain _brain;
    private bool _hasFinishedInstructions;

    public DarwinBrain Brain => _brain;
    public bool HasFinishedInstructions => _hasFinishedInstructions;
    
    private bool _isWaitingToStartAction = true;
    private bool _actionStarted = false;
    private float _aiActionTimer = 0f;
    private float _aiMaxActionTimer = 0f;
    private float _bestHeightReached = 0f;
    private float _fitness = 0f;
    private int _reachedBestAtStepNr = 0;
    private DarwinAction _currentAction = null;
    private int _brainActionNr = 0;

    public float Fitness => _fitness;
    public float BestHeightReached => _bestHeightReached;



    public void LoadState(PlayerInfo playerInfo)
    {
        // public DarwinBrain brain;
        // public Vector3 position;
        // public bool isGrounded;
        // public float bestHeightReached;
        // public int bestHeightReachedAtStepNr;
        // public int brainActionNr;
        // public DarwinDirection direction;
        // public bool isWaitingToStartAction;
        // public bool startedAction;

        _brain = playerInfo.brain;
        transform.position = playerInfo.position;
        _isGrounded = playerInfo.isGrounded;
        _bestHeightReached = playerInfo.bestHeightReached;
        _reachedBestAtStepNr = playerInfo.bestHeightReachedAtStepNr;
        _brainActionNr = playerInfo.brainActionNr;
        _dir = playerInfo.moveDir;
        _isWaitingToStartAction = playerInfo.isWaitingToStartAction;
        _actionStarted = playerInfo.startedAction;
    }

    public PlayerInfo GetPlayerState()
    {
        PlayerInfo playerInfo = new PlayerInfo();
        
        playerInfo.brain = _brain.Clone();
        playerInfo.position = transform.position;
        playerInfo.isGrounded = _isGrounded;
        playerInfo.bestHeightReached = _bestHeightReached;
        playerInfo.bestHeightReachedAtStepNr = _reachedBestAtStepNr;
        playerInfo.brainActionNr = _brainActionNr;
        playerInfo.moveDir = _dir;
        playerInfo.isWaitingToStartAction = _isWaitingToStartAction;
        playerInfo.startedAction = _actionStarted;

        return playerInfo;
    }

    public void CalculateFitness()
    {
        // TODO: add level height
        _fitness = _bestHeightReached;
    }

    public float GetHeight()
    {
        return transform.position.y;
    }

    public PlayerInfo Clone()
    {
        return GetPlayerState();
    }

    public void UpdateHighestPointReached()
    {
        if (GetHeight() > _bestHeightReached && _isGrounded)
        {
            _bestHeightReached = GetHeight();
            _reachedBestAtStepNr = _brain.CurrentInstructionId;
        }
    }

    void Start()
    {
        _brain = new DarwinBrain(5);
        _currentAction = _brain.GetNextAction();
        
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

    public void UpdateDarwinAction()
    {
        if (_isWaitingToStartAction && _isGrounded)
        {
            _isWaitingToStartAction = false;
        }

        if (_isGrounded && !_actionStarted)
        {
            _currentAction = _brain.GetNextAction();

            if (_currentAction == null)
            {
                _hasFinishedInstructions = true;
                return;
            }

            StartCurrentAction();
            _actionStarted = true;
        }
        else if (_actionStarted)
        {
            _aiActionTimer += Time.deltaTime;

            if (_aiActionTimer >= _aiMaxActionTimer)
            {
                EndCurrentAction();
                
                _actionStarted = false;
            }
        }
    }

    public void StartCurrentAction()
    {
        _aiMaxActionTimer = Mathf.Floor(_currentAction.HoldTime);
        _aiActionTimer = 0;

        if (_currentAction.IsJump)
        {
            _cm.IsHoldingJump = true;
        }

        if (_currentAction.Direction == MoveDirection.Left)
        {
            _cm.Movement = -1f;
        }
        else if (_currentAction.Direction == MoveDirection.Right)
        {
            _cm.Movement = 1f;
        }
    }

    public void EndCurrentAction()
    {
        if (_currentAction.IsJump && _isGrounded)
        {
            _cm.IsHoldingJump = false;
            Jump();
        }

        _cm.Movement = 0;
        _isWaitingToStartAction = false;
    }

    public void Jump()
    {
        //MapJumpForce(input);
            
        // Set animator sprite info
        _an.SetBool("HasPressedJump", false);
            
        // If grounded apply jump
        if (_isGrounded)
        {
           
            float xDistance = _cm.Movement * _walkSpeed * _distanceMultiplier;
            float yDistance = _jumpValue;

            var displacement = new Vector2(xDistance, yDistance);
           
            _currentFrameVelocity = displacement;
            //_rb.AddForce(new Vector2(xDistance, 0), ForceMode2D.Impulse);
                
           Invoke("ResetJump", 0.2f);
        }

        // // Reset data
        // _jumpValue = 0f;
        // _jumpTime = 0f;
        // _canJump = true;
    }

    // Update is called once per frame
    void Update()
    {
        _currentFrameVelocity = _rb.velocity;
        //
        // // Code for checking slanted areas
        // RaycastHit2D hit = new RaycastHit2D();
        // if (PlayerIsOnSlatedFloor(ref hit))
        // {
        //     ApplySlipping(hit);
        //     return;
        // }

        // Animator checks
        UpdateAnimatorMoveDirection(_cm.Movement);
        UpdateAnimatorGrounded(_cm.Movement);
        
        // Ai check
        UpdateHighestPointReached();
        
        // Ground collision check
        UpdateIsGrounded();

        // If player is grounded and didn't jump
        if (_jumpValue <= float.Epsilon && _isGrounded)
        {
            _currentFrameVelocity = new Vector2(_cm.Movement * _walkSpeed, _rb.velocity.y);
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
            _currentFrameVelocity = new Vector2(0f, _rb.velocity.y);
        }

        #if !_USER_INPUT
        UpdateDarwinAction();
        #endif
        
        // // When jump is held maximum amount trigger jump
        // if (_jumpTime >= _maxJumpTime && _isGrounded)
        // {
        //     // Set sprite animator parameter
        //     _an.SetBool("HasPressedJump", false);
        //     
        //     // Get values and apply them
        //     float tempX = input * _walkSpeed * _distanceMultiplier;
        //     float tempY = _jumpValue;
        //     
        //     _rb.velocity = new Vector2(tempX, tempY);
        //     
        //     // Reset jump info once applied to rb
        //     Invoke("ResetJump", 0.2f);
        // }

        #if _USER_INPUT
        if (_cm.HasReleasedJump && _isGrounded)
        {
            Jump();
        }
        #endif
        // Premature release
        // if (_cm.HasReleasedJump)
        // {
        //     //MapJumpForce(input);
        //     
        //     // Set animator sprite info
        //     _an.SetBool("HasPressedJump", false);
        //     
        //     // If grounded apply jump
        //     if (_isGrounded)
        //     {
        //         float xDistance = input * _walkSpeed * _distanceMultiplier;
        //         float yDistance = _jumpValue;
        //
        //         var displacement = new Vector2(xDistance, yDistance);
        //         Debug.Log(displacement);
        //         
        //         _rb.velocity = new Vector2(xDistance, yDistance);
        //         //_rb.AddForce(new Vector2(xDistance, 0), ForceMode2D.Impulse);
        //         
        //         _jumpValue = 0f;
        //     }
        //
        //     // Reset data
        //     _shortJumpValue = 1f;
        //     _jumpTime = 0f;
        //     _canJump = true;
        // }
        
        _rb.velocity = _currentFrameVelocity;
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
        _currentFrameVelocity = velocity;
    }

    void ResetJump()
    {
        _jumpValue = 0f;
        _jumpTime = 0f;
        _canJump = true;
    }
}
