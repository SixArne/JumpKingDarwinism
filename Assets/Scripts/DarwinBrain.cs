using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Random = UnityEngine.Random;

public class DarwinBrain
{
    private List<DarwinAction> _actions = new List<DarwinAction>();
    private int _currentInstructionNumber = 0;
    private readonly float _jumpChance = 0.5f;
    private readonly float _jumpChanceFull = 0.2f;
    private int _parentReachedBestLevelAtActionNo = 0;

    public int CurrentInstructionNumber => _currentInstructionNumber;

    public int ParentReachedBestLevelAtActionNo
    {
        get => _parentReachedBestLevelAtActionNo;
        set => _parentReachedBestLevelAtActionNo = value;
    }

    public List<DarwinAction> Actions
    {
        get => _actions;
        set => _actions = value;
    }

    public DarwinBrain(int size, bool shouldRandomize = true)
    {
        if (shouldRandomize)
        {
            Randomize(size);
        }

    }

    public void Randomize(int size)
    {
        for (int i = 0; i < size; i++)
        {
            _actions.Add(GetRandomAction());
        }
    }

    public DarwinAction GetRandomAction()
    {
        bool canJump = false;

        if (Random.Range(0f, 1f) > _jumpChance)
        {
            canJump = true;
        }

        float holdTime = Random.Range(0.1f, 1f);

        if (Random.Range(0f, 1f) < _jumpChanceFull)
        {
            holdTime = 1.0f;
        }

        
        MoveDirection[] options = {MoveDirection.Left, MoveDirection.Left, MoveDirection.Left,
            MoveDirection.None, MoveDirection.Right, MoveDirection.Right, MoveDirection.Right };
        int randomDir = Random.Range(0, 7);

        MoveDirection direction = (MoveDirection)options[randomDir];

        return new DarwinAction(canJump, holdTime, direction);
    }

    public DarwinAction GetNextAction()
    {
        if (_currentInstructionNumber >= _actions.Count)
        {
            return null;
        }

        _currentInstructionNumber++;
        return _actions[_currentInstructionNumber - 1];
    }

    public DarwinBrain Clone()
    {
        var clone = new DarwinBrain(_actions.Count, false);
        clone.Actions = new List<DarwinAction>();

        for (int i = 0; i < _actions.Count; i++)
        {
            clone.Actions.Add(_actions[i].Clone());
        }

        return clone;
    }

    public void Mutate()
    {
        float mutationRate = 0.1f;
        float chanceOfNewInstruction = 0.02f;
        for (int i = _parentReachedBestLevelAtActionNo; i < _actions.Count; i++)
        {
            if (Random.Range(0f, 1f) < chanceOfNewInstruction)
            {
                _actions[i] = GetRandomAction();
            } 
            else if (Random.Range(0f, 1f) < mutationRate)
            {
                _actions[i].Mutate();
            }
        }
    }

    public void MutateActionNumber(int actionId)
    {
        actionId -= 1;
        float chanceOfNewInstruction = 0.3f;
        if (Random.Range(0f, 1f) < chanceOfNewInstruction)
        {
            _actions[actionId] = GetRandomAction();
        }
        else
        {
            _actions[actionId].Mutate();
        }
    }

    public void IncreaseMoves(int increaseBy)
    {
        for (int i = 0; i < increaseBy; i++)
        {
            _actions.Add(GetRandomAction());
        }
    }
}
