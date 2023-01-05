using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class DarwinPopulation : MonoBehaviour
{
    [SerializeField] private int _populationSize;
    [SerializeField] private GameObject _agentPrefab;
    [SerializeField] private int _increaseEveryXGenerations = 5;
    [SerializeField] private int _increaseActionsByAmount = 5;
    [SerializeField] private Transform _spawnLocation;


    private List<PlayerController2D> _players = new List<PlayerController2D>();
    private List<DarwinAction> _bestActions = new List<DarwinAction>();
    private Vector3 _bestPlayerPosition = Vector3.zero;

    private PlayerInfo _cloneOfBestPlayerPrevGen;
    private int _generation = 1;
    private int _bestPlayerIndex = 0;
    private bool _newLevelReached = false;
    private float _fitnessSum = 0f;
    private int _currentHeighestPlayer = 0;
    private float _bestHeight;
    private int _moveAmount = 5;

    public int Generation => _generation;
    public int PlayerCount => _players.Count;
    public float BestHeight => _bestHeight;

    public int Moves => _moveAmount;

    void Awake()
    {
        for (int i = 0; i < _populationSize; i++)
        {
            GameObject result = Instantiate(_agentPrefab);
            PlayerController2D player = result.GetComponent<PlayerController2D>();

            _players.Add(player);
            _bestPlayerPosition = result.transform.position;
        }
    }

    void Update()
    {
        if (AllPlayersFinished())
        {
            // When we have trained the first 5 moves enough
            // copy all best actions and player position
            // and restart training.
            if (_generation % _increaseEveryXGenerations == 0)
            {
                // Save the last gens player best moves
                SaveBestMoves();

                // Generate new players with just position of last player
                // actions are already saved
                GenerateGenerationForNextSection();

                // avoid instant natural selection by early exit
                return;
            }

            // Modify existing DNA
            NaturalSelection();
        }
    }

    public void GenerateGenerationForNextSection()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            Destroy(_players[i].gameObject);

            GameObject newAgent = Instantiate(_agentPrefab, _bestPlayerPosition, Quaternion.identity);
            PlayerController2D player = newAgent.GetComponent<PlayerController2D>();

            _players[i] = player;
        }

        _generation++;
    }

    public void SaveBestMoves()
    {
        var bestPlayerActions = _players[0].Brain.Actions;

        foreach (var action in bestPlayerActions)
        {
            _bestActions.Add(action);
        }

        _bestPlayerPosition = _players[0].BestPosition;
    }
    void NaturalSelection()
    {
        List<PlayerInfo> nextGeneration = new List<PlayerInfo>();

        SetBestPlayer();
        CalculateFitnessSum();

        _cloneOfBestPlayerPrevGen = _players[_bestPlayerIndex].Clone();
        nextGeneration.Add(_players[_bestPlayerIndex].Clone());

        for (int i = 1; i < _players.Count; i++)
        {
            PlayerController2D parent = SelectParent();
            PlayerInfo baby = parent.Clone();

            if (parent.FellToPreviousLevel)
            {
                baby.brain.MutateActionNumber(parent.FellOnActionNr);
            }

            baby.brain.Mutate();
            nextGeneration.Add(baby);

            // Destroy old
            Destroy(_players[i].gameObject);
        }
        Destroy(_players[0].gameObject);

        for (int i = 0; i < nextGeneration.Count; i++)
        {
            GameObject newAgent = Instantiate(_agentPrefab, _bestPlayerPosition, Quaternion.identity);
            PlayerController2D player = newAgent.GetComponent<PlayerController2D>();
            player.LoadState(nextGeneration[i]);

            _players[i] = player;
        }

        _generation++;
    }

    void CalculateFitnessSum()
    {
        _fitnessSum = 0f;
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

    public int CountFullJumps()
    {
        int counter = 0;
        List<PlayerController2D> players = new List<PlayerController2D>();

        foreach (var player in _players)
        {
            if (player.Brain.Actions[0].HoldTime > 0.9f && player.Brain.Actions[0].Direction != 0 && player.Brain.Actions[0].IsJump)
            {
                counter++;
                players.Add(player);
            }
        }

        return counter;
    }

    public int CountWalks()
    {
        int counter = 0;
        List<PlayerController2D> players = new List<PlayerController2D>();

        foreach (var player in _players)
        {
            if (!player.Brain.Actions[0].IsJump)
            {
                counter++;
                players.Add(player);
            }
        }

        return counter;
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

        _bestHeight = _players[_bestPlayerIndex].BestHeightReached;
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

        _moveAmount = _players[0].Brain.Actions.Count;
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

    
}
