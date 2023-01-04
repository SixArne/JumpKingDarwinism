using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class DarwinPopulation : MonoBehaviour
{
    [SerializeField] private int _populationSize;
    [SerializeField] private GameObject _agentPrefab;
    [SerializeField] private int _increaseEveryXGenerations = 5;
    [SerializeField] private int _increaseActionsByAmount = 5;
    
    private List<PlayerController2D> _players = new List<PlayerController2D>();
    private PlayerInfo _cloneOfBestPlayerPrevGen;
    private int _generation = 0;
    private int _bestPlayerIndex = 0;
    private bool _newLevelReached = false;
    private float _fitnessSum = 0f;
    private int _currentHeighestPlayer = 0;

    public int Generation => _generation;
    public int PlayerCount => _players.Count;

    void Awake()
    {
        for (int i = 0; i < _populationSize; i++)
        {
            var result = Instantiate(_agentPrefab);
            var component = result.GetComponent<PlayerController2D>();

            _players.Add(component);
        }
    }

    void Update()
    {
        if (AllPlayersFinished())
        {
            NaturalSelection();

            if (_generation % _increaseEveryXGenerations == 0)
            {
                IncreasePlayerMoves(_increaseActionsByAmount);
            }
        }
    }

    void SetBestPlayer()
    {
        _bestPlayerIndex = 0;
        _newLevelReached = false;

        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].BestHeightReached > _players[_bestPlayerIndex].BestHeightReached)
            {
                _bestPlayerIndex = i;
            }
        }
    }

    void SetCurrentHighestPlayer()
    {
        _currentHeighestPlayer = 0;
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].GetHeight() > _players[_currentHeighestPlayer].GetHeight())
            {
                _currentHeighestPlayer = i;
            }
        }
    }

    void Show()
    {
        
    }

    void ResetAllPlayers()
    {
        // for (int i = 0; i < _players.Count; i++)
        // {
        //     _players[i].Reset();
        // }
    }

    void IncreasePlayerMoves(int increaseBy)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            _players[i].Brain.IncreaseMoves(increaseBy);
        }
    }

    bool AllPlayersFinished()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (!_players[i].HasFinishedInstructions)
            {
                return false;
            }
        }

       
        return true;
    }

    void NaturalSelection()
    {
        List<PlayerInfo> nextGeneration = new List<PlayerInfo>();
        
        SetBestPlayer();
        CalculateFitnessSum();

        _cloneOfBestPlayerPrevGen = _players[_bestPlayerIndex].Clone();
        nextGeneration.Add( _players[_bestPlayerIndex].Clone());

        for (int i = 0; i < _players.Count; i++)
        {
            PlayerController2D parent = SelectParent();
            PlayerInfo baby = parent.Clone();
            
            baby.brain.Mutate();
            nextGeneration.Add(baby);
        }

        for (int i = 0; i < _players.Count; i++)
        {
            // Destroy old
            Destroy(_players[i].gameObject);

            GameObject newAgent = Instantiate(_agentPrefab);
            PlayerController2D player = newAgent.GetComponent<PlayerController2D>();
            player.LoadState(nextGeneration[i]);

            _players[i] = player;
        }
        
        _generation++;
        
        Debug.Log(string.Format("generation: {0}", _generation));
    }

    void CalculateFitnessSum()
    {
        this._fitnessSum = 0f;
        for (int i = 0; i < _players.Count; i++)
        {
            _players[i].CalculateFitness();
            _fitnessSum += _players[i].Fitness;
        }
    }

    PlayerController2D SelectParent()
    {
        float rand = Random.Range(0, _fitnessSum);
        float runningSum = 0f;
        for (int i = 0; i < _players.Count; i++)
        {
            runningSum += _players[i].Fitness;
            if (runningSum > rand)
            {
                return _players[i];
            }
        }
        
        return null;
    }
}
