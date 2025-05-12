# Simple Agent Model Simulator

## How to use

- Download executable from the last release in releases tab.
- Open command line in the same directory as is the executable.
- Run the program with adequate command line arguments.

When you run the program without any argument, it will describe possibilities.

```bash
> SimpleAgentModel-{os} {args}
```

For example on Linux, after downloading executable:

```bash
> ls
SimpleAgentModel-linux-x64 OtherFiles ...
> chmod +x SimpleAgentModel-linux-x64
> ./SimpleAgentModel-linux-x64 path:./MyAgent x:30 y:12
```

This will make the file executable and run it with arguments.

On Windows after downloading:

```"
> SimpleAgentModel-win-x64.exe path:MyAgent x:30 y:12
```

## What it does

- Loads your agent from file. How to write agent is described in Example.agent or below.
- Creates grid of agents, with 8-neighbours system.
- Run the simulation.

You can specify beginning constraints, or rather just one line.
That is good for at least one agent model - forest burning.I

You can use agents which are already created in file Agents.

## How to write agent file

Create new text file. Here are contents of Example.agent:

```"
0,1,2               // All possible states of agent - must be on line 1
0.5, 0.4, 0.1       // Probabilities of all states - must be on line 2
6                   // Number of rules, on the following lines - must be on line 3
0 -> 1              // Simple rule: if the state is 0, change to 1 - rules must be after line 3.
0 -> 2              // The simple rules have precedence before complex rules. But this rule won't ever be used.
min 2: 0 : 1 -> 2   // If in the 8-neighbourhood are at least 2 agents with State = 0, and this agent is state 1, this agent changes to 2.
max 3: 2 : 1 -> 0   // If at most 3 agents of State = 1, change to 0. All rules are evaluated sequentially, top to bottom.
is 4 : 1 : 2 -> 0   // if exactly 4 agents of state 1 are neighbours and this cell is 2, it becomes 0
default: x -> (x + 1) % 3   // If no rule is triggered, change the state according to this rule.
// You can add comments on the end of any line.
// After you finished all rules, the rest of file will be ignored.
// You can choose possible states based on the colors, if you want.
State 	Color
0    	Black
1    	Red
2   	Green
3    	Yellow
4    	Blue
5    	Purple
6    	Cyan
7   	White
// You can use any integer as a state, but only these would have different colors.

Simple rules have precedence.
All rules are saved and evaluated in the same order as you write them down.
```
