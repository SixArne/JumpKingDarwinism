

//#define _USER_INPUT
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class PlayerInfo
{
    public DarwinBrain brain;
    public bool isGrounded;
    
    public float bestHeightReached;
    public int bestLevelReached;
    public int reachedHeightAtStepNr;
    public int bestLevelReachedOnActionNr;
    
    public int brainActionNumber;
    public MoveDirection moveDirection;
    
    public bool isWaitingToStartAction;
    public bool startedAction;
    public Vector3 bestPosition;
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
    [SerializeField] private int _initialInstructionSize = 5;

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
    private float _bestHeightReached = -0.5f;
    private int _bestLevelReached = 0;
    private float _fitness = 0f;
    private int _reachedHeightAtStepNr = 0;
    private int _bestLevelReachedOnActionNr = 0;
    private DarwinAction _currentAction = null;
    private int _brainActionNr = 0;
    private bool _fellToPreviousLevel = false;
    private int _fellOnActionNr = 0;
    private bool _actionIsFinished = true;
    private Vector3 _bestPosition = Vector3.zero;

    public float Fitness => _fitness;
    public float BestHeightReached => _bestHeightReached;

    public bool FellToPreviousLevel => _fellToPreviousLevel;
    public int FellOnActionNr => _fellOnActionNr;

    public Vector3 BestPosition => _bestPosition;



    public void LoadState(PlayerInfo playerInfo)
    {
        _brain = playerInfo.brain;
        _currentAction = _brain.GetRandomAction();
        _isGrounded = playerInfo.isGrounded;
        
        _bestHeightReached = playerInfo.bestHeightReached;
        _bestLevelReached = playerInfo.bestLevelReached;
        _reachedHeightAtStepNr = playerInfo.reachedHeightAtStepNr;
        _bestLevelReachedOnActionNr = playerInfo.bestLevelReachedOnActionNr;
        _bestPosition = playerInfo.bestPosition;
        
        _brainActionNr = playerInfo.brainActionNumber;
        _dir = playerInfo.moveDirection;
        _isWaitingToStartAction = playerInfo.isWaitingToStartAction;
        _actionStarted = playerInfo.startedAction;
    }

    public PlayerInfo GetPlayerState()
    {
        PlayerInfo playerInfo = new PlayerInfo();
        
        playerInfo.brain = _brain.Clone();
        playerInfo.brain.ParentReachedBestLevelAtActionNo = _reachedHeightAtStepNr;
        playerInfo.isGrounded = _isGrounded;
        
        playerInfo.bestHeightReached = _bestHeightReached;
        playerInfo.bestLevelReached = _bestLevelReached;
        playerInfo.reachedHeightAtStepNr = _reachedHeightAtStepNr;
        playerInfo.bestLevelReachedOnActionNr = _bestLevelReachedOnActionNr;
        playerInfo.bestPosition = _bestPosition;

        playerInfo.brainActionNumber = _brainActionNr;
        playerInfo.moveDirection = _dir;
        playerInfo.isWaitingToStartAction = _isWaitingToStartAction;
        playerInfo.startedAction = _actionStarted;

        return playerInfo;
    }

    public void CalculateFitness()
    {
        //_fitness = _bestHeightReached * (_brain.Actions.Count / (_reachedHeightAtStepNr + 0.1f));
        // 50 * (5 / 1) => 250
        // 50 * (5 / 2) => 125
        _fitness = _bestHeightReached;
    }

    public float GetHeight()
    {
        return transform.position.y;
    }

    public PlayerInfo Clone()
    {
        PlayerInfo info = new PlayerInfo();
        info.brain = _brain.Clone();
        info.brain.ParentReachedBestLevelAtActionNo = _bestLevelReachedOnActionNr;

        info.bestHeightReached = _bestHeightReached;
        info.bestLevelReached = _bestLevelReached;
        info.reachedHeightAtStepNr = _reachedHeightAtStepNr;
        info.bestLevelReachedOnActionNr = _bestLevelReachedOnActionNr;
        info.brainActionNumber = _brainActionNr;
        info.bestPosition = _bestPosition;

        info.moveDirection = _dir;

        return info;
    }

    public void UpdateHighestPointReached()
    {
        var currentHeight = GetHeight();

        if (currentHeight > _bestHeightReached && _isGrounded)
        {
            _bestHeightReached = currentHeight;
            _reachedHeightAtStepNr = _brain.CurrentInstructionNumber;
            _brain.ParentReachedBestLevelAtActionNo = _reachedHeightAtStepNr;
            _bestPosition = transform.position;
        }
        else if (currentHeight < _bestHeightReached - 1f && _isGrounded)
        {
            _fellToPreviousLevel = true;
            _fellOnActionNr = _brain.CurrentInstructionNumber;

            //// shortcirtuit 
            //_hasFinishedInstructions = true;
        }
    }

    void Start()
    {
        if (_brain == null)
        {
            _brain = new DarwinBrain(_initialInstructionSize);
        }

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

    void Finish()
    {
        _hasFinishedInstructions = true;
    }

    public void UpdateDarwinAction()
    {
        if (_hasFinishedInstructions)
        {
            return;
        }

        if (_isWaitingToStartAction && _isGrounded)
        {
            _rb.velocity = Vector2.zero;
            _isWaitingToStartAction = false;
        }

        if (_isGrounded && !_actionStarted)
        {
            _currentAction = _brain.GetNextAction();

            if (_currentAction == null)
            {
                Invoke("Finish", 2.0f);

                return;
            }

            StartCurrentAction();

            if (_currentAction.IsJump)
            {
                _an.SetBool("HasPressedJump", true);
            }
            else
            {
                _an.SetBool("HasPressedJump", false);
            }

            _actionIsFinished = false;
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
        else
        {
            _cm.IsHoldingJump = false;
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
        _an.SetBool("HasPressedJump", false);

        if (_currentAction.IsJump && _isGrounded)
        {
            _cm.IsHoldingJump = false;
            Jump();

            Invoke("InvokedWaitingToStartAction", 0.1f);
        }
        
        _cm.Movement = 0;
        //_isWaitingToStartAction = false;
    }
    
    void InvokedWaitingToStartAction()
    {
        _isWaitingToStartAction = true;
        //_cm.Movement = 0;
    }

    public void Jump()
    {
            
        // Set animator sprite info
        _an.SetBool("HasPressedJump", false);
            
        // If grounded apply jump
        if (_isGrounded)
        {
           
            float xDistance = _cm.Movement * _walkSpeed * _distanceMultiplier;
            float yDistance = _jumpValue;

            var displacement = new Vector2(xDistance, yDistance);
           
            _currentFrameVelocity = displacement;

            //_rb.AddForce(displacement, ForceMode2D.Impulse);
            _rb.velocity = _currentFrameVelocity;

            _jumpValue = 0f;
            _jumpTime = 0f;
            _canJump = true;
            //Invoke("ResetJump", 0.2f);
        }
    }

    void ResetJump()
    {
        _currentFrameVelocity = Vector2.zero;
        _jumpValue = 0f;
        _jumpTime = 0f;
        _canJump = true;
    }

    // Update is called once per frame
    void Update()
    {
        //_currentFrameVelocity = new Vector2(0, _rb.velocity.y);
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
        
        // Ground collision check
        UpdateIsGrounded();

        // Ai check
        UpdateHighestPointReached();

        // Bounciness material switch
        SwitchMaterialOnJump();

        // Update current action
        UpdateDarwinAction();

        // to avoid errors
        if (_currentAction == null)
            return;

        if (!_currentAction.IsJump && _isGrounded)
        {
            _rb.velocity = new Vector2(_cm.Movement * _walkSpeed, 0);
        }

        // If player is holding jump wind up jump power
        if (_currentAction.IsJump && _isGrounded)
        {
            // Set sprite animator parameter
            _an.SetBool("HasPressedJump", true);

            _jumpTime += Time.deltaTime;
            _jumpValue = Mathf.Lerp(_minJumpValue, _maxJumpValue, _jumpTime / _maxJumpTime);
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
            new Vector2(.5f, .3f), 0f, _groundMask
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

  
}
