using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DarwinBrain
{
    private List<DarwinAction> _actions = new List<DarwinAction>();
    private int _currentInstructionId = 0;
    private readonly double _jumpChance = 0.5;
    private readonly double _jumpChanceFull = 0.2;

    public int CurrentInstructionId => _currentInstructionId;

    public List<DarwinAction> Actions
    {
        get => _actions;
        set => _actions = value;
    }

    public DarwinBrain(int size)
    {
        Randomize(size);
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
        Random random = new Random();

        if (random.NextDouble() > _jumpChance)
        {
            canJump = true;
        }

        double holdTime = random.NextDouble();

        if (random.NextDouble() < _jumpChanceFull)
        {
            holdTime = 1.0;
        }

        int randomDir = random.Next(1, 3);
        MoveDirection direction = (MoveDirection)randomDir;

        return new DarwinAction(canJump, (float)holdTime, direction);
    }

    public DarwinAction GetNextAction()
    {
        if (_currentInstructionId >= _actions.Count)
        {
            return null;
        }

        _currentInstructionId++;
        return _actions[_currentInstructionId - 1];
    }

    public DarwinBrain Clone()
    {
        var clone = new DarwinBrain(_actions.Count);
        clone.Actions = new List<DarwinAction>();

        for (int i = 0; i < _actions.Count; i++)
        {
            clone.Actions.Add(_actions[i].Clone());
        }

        return clone;
    }

    public void Mutate()    
    {

    }

    public void MutateActionNumber(int actionId)
    {
        
    }

    public void IncreaseMoves(int increaseBy)
    {
        
    }
}
