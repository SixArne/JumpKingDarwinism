![jumking banner alt](/jumpking.jpg)

# Genetic algorithm Jumpking

This research project will be about the genetic algorithm, in this short paper I will attempt to 
recreate the jumpking game and add an AI to the agent, the end goal for this AI is to beat the level in an
optimal manner.

## About jumpking

For those that don't know jumpking, jumpking is a platformer game where the players controls a simple agent that has to
jump to make it to the top of the map where he can acquire "the Babe".

## Controls

The controls are fairly simple, the player can move left, right and jump up. If you hold the left or right key while jumping
the character will jump in that direction instead.

![movement](/movement.gif)

:::danger Disclaimer
It might seem like easy controls but by holding down the space bar the player winds up a jump, but there is no
visual feedback on how long the player has held the key, making it difficult to remember how long a key
has to be held to make a jump.

On top of all that the player can also bounce of a wall when he hits it.
:::



## About the evolutionary algorithm

There are different forms of AI that be be used in this project, but the one I will use is called `Evolutionary Algorithm`
which is part of the bigger Genetic Algorithms.

This algorithm will use a neural network and essentially refine its network based on a reward system. In the case of
jump king a good reward would be getting higher, and a bad reward would be falling down. Now the most optimal thing to do here is
to run hundreds of instances at the same time and pick out the best agent, then we spawn a 100 more with the same neural network
as last generation's winner and we repeat this process. The algorithm itself has a few different steps.

## Initialization

First we will generate a list of random actions for every agent, these can range from jumping to walking. Every action will have a 
`hold time`, this garantuees that the AI will perform long and short jumps.

These actions will then be executed in order.

```csharp
public DarwinAction(bool isJump, float holdTime, MoveDirection direction)
{
    _isJump = isJump;
    _holdTime = holdTime;
    _direction = direction;
}
```

### Creating the brain

The brain will hold a list of actions/instructions, the brain will also be resposible for mutating the actions later on in the `Natural selection` stage.

It also holds a `Clone` method which is required for the reproducing stage of the algorithm.

```csharp
public class DarwinBrain
{
    private List<DarwinAction> _actions = new List<DarwinAction>();
    private int _currentInstructionNumber;
    
    ...

    public DarwinBrain(int size, bool shouldRandomize = true)
    {
        ...
    }

    public void Randomize(int size)
    {
        ...
    }

    public DarwinAction GetRandomAction()
    {
        ...
    }

    public DarwinAction GetNextAction()
    {
        ...
    }

    public DarwinBrain Clone()
    {
        ...
    }

    public void Mutate()
    {
        ...
    }
}

```

## Running

After successfully initializing the agents we let them run through their action pool, most of the early jumps will seem random in the beginning but after a while it should learn how to jump correctly, although this process can take a long time.

![Random movement](/random_movement.gif)

## Natural Selection

After running through all agent agents we start the stage known as `Natural Selection ~ Charles Darwin`. We will go over every agent and decide if it did good or bad. This is done through the `Fitness function`.

This funtion returns a high number when it did something good and a low number when it did something bad. In Jumpking's case
it's the height of the player, the heigher it reached the better it did.

```csharp
public void CalculateFitness()
{
    _fitness = _bestHeightReached;
}
```

And finally we go over all agents, if the agent has done well we use his DNA (or bytes) to create new agents. This is done through the clone functionality. Only the best player of previous generation gets put back in the gene pool. All other agents get modified.

```csharp
void NaturalSelection()
    {
        // Define the list for the next generation
        List<PlayerInfo> nextGeneration = new List<PlayerInfo>();
        
        // Find the best agent of the current generation
        SetBestPlayer();

        // Calculate the combined Fitness of all agents
        CalculateFitnessSum();

        // We clone the best agent
        _cloneOfBestPlayerPrevGen = _players[_bestPlayerIndex].Clone();
        nextGeneration.Add(_cloneOfBestPlayerPrevGen);

        for (int i = 0; i < _players.Count; i++)
        {
            // We don't want to modify our best agent
            if (i == _bestPlayerIndex)
            {
                continue;
            }

            // Select a new parent for the player
            PlayerController2D parent = SelectParent();
 
            // Get the important information
            PlayerInfo baby = parent.Clone();

            // Force a mutation if the player fell down
            if (parent.FellToPreviousLevel)
            {
                baby.brain.MutateActionNumber(parent.FellOnActionNr);
            }
            
            // Mutate the cloned information
            baby.brain.Mutate();

            // Add the mutated brain to the list
            nextGeneration.Add(baby);
        }
        
        _generation++;
    }
```

## Optimizations

Currently there is an issue with the way the AI works.

The AI will continue to learn new actions without optimizing the old ones. This results in unnessecary actions and wasted movement. It will also consume more and more RAM usage from the computer.

But the biggest issue with this algorithm is that it wastes too much time on doing the same moves over and over. Instead we can let them work on the first 5 moves, and then instantiate them all on the best reached location. This will also result in a faster training time.

```csharp
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
```

## Timelapse

<iframe width="700" height="315" src="https://www.youtube.com/embed/Kjh1ZynGH50" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

## Closing words

After writing this I noticed that this form of AI behavior is very situational as it only
works on static maps, meaning that if I were to suddenly take a different map it would not work
at all as it is trained for that once specific scenario.

This project was very difficult to make, having no experience in anything AI related made it very hard for me to pinpoint
what I had to do and how to improve the algorithm, not having a lot of experience in Unity
also resulted in some bugs that could have been avoided.

So my future goals are learning more about Unity and making games in them and deep diving
more in general AI programming.

I would like to thank `Rune` for the advice he gave me on how to pinpoint these issues
and `Codebullet` for having his implementation on github so I could analyse everything and therefore learn how the algorithm workes.

