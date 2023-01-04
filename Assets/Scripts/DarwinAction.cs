
using UnityEngine;

public class DarwinAction
{
    private bool _isJump;
    private float _holdTime;
    private MoveDirection _direction;

    public float HoldTime => _holdTime;

    public bool IsJump => _isJump;

    public MoveDirection Direction => _direction;

    public DarwinAction(bool isJump, float holdTime, MoveDirection direction)
    {
        _isJump = isJump;
        _holdTime = holdTime;
        _direction = direction;
    }

    public DarwinAction Clone()
    {
        return new DarwinAction(_isJump, _holdTime, _direction);
    }
    
    public void Mutate()
    {
        _holdTime += Random.Range(-0.3f, 0.3f);
        _holdTime = Mathf.Clamp(_holdTime, 0.1f, 1f);
    }
}
