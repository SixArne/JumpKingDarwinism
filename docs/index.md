![jumking banner alt](/jumpking.jpg)

# JumpKingAI

This research project will be about the evolutionary algorithm, in this short paper I will attempt to 
recreate the jumpking game and add an AI to the agent, the end goal for this AI is to beat the level in an
optimal manner.

## About jumpking

For those that don't know jumpking, jumpking is a rage game where the players controls a simple agent that has to
jump to make it to the top of the map where he can acquire "the Babe".

### Controls

The controls are fairly simple, the player can:

- Jump
- Move left and right
- Jump left or right

It might seem like easy controls but by holding down the space bar the player winds up a jump, but there is no
visual feedback on how long the player has held the key, making it difficult to remember how long a key
has to be held to make a jump.

On top of all that the player can also bounce of a wall when he hits it.

## About the evolutionary algorithm

There are different forms of AI that be be used in this project, but the one I will use is called `Evolutionary Algorithm`
which is part of the bigger Genetic Algorithms.

This algorithm will use a neural network and essentially refine its network based on a reward system. In the case of
jump king a good reward would be getting higher, and a bad reward would be falling down. Now the most optimal thing to do here is
to run hundreds of instances at the same time and pick out the best agent, then we spawn a 100 more with the same neural network
as last generation's winner and we repeat this process.

This algorithm will not use any external libraries, everything willen we written in pure C# but can be put in any other language.

## The algorithm in "understandable language"

### Setup:  

We first define some good rewards and some bad rewards, if my agent was a human child that would
certainly hurt my wallet. Luckily for us the agent is a computer who's super happy with receiving a number.
this will helps us to have an appendix expanding on how these "children" are nothing more but 1 and 0's and don't deserve any equal treatment.

So lets abuse this ignorance and put this agent to slave labour.

I could have also written this with deep learning, but that would be a lot more difficult especially for a newbie like me.
So these children don't need eyes, they have no right to them.

### Step 1:

We clone these naive boys until we have a hundred of them and then procees to put them
into a space that is too small for them to breath in.

### Step 2: 

We tell them to do random shit and after some time we go over all our children, the ones that
performed badly get executed while we put the winners together to make more babies.

### Step 3:

And repeat until a child is fortunate enough to beat the current level.