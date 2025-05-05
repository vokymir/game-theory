
namespace SimpleAgentModel;

/// <summary>
/// Store grid of agents,
/// show it in the console.
/// </summary>
public class Grid<TAgent> where TAgent : Agent, new()
{
    public TAgent[,] Agents { get; set; }
    private Random _rnd = new Random();

    public Grid(int x, int y)
    {
        Agents = new TAgent[x, y];
    }

    public void RandomizeGrid(int[] possibleStates)
    {
        for (int x = 0; x < Agents.GetLength(0); x++)
        {
            for (int y = 0; y < Agents.GetLength(1); y++)
            {
                var agent = Agents[x, y];
                if (agent is null)
                {
                    agent = new TAgent();
                    Agents[x, y] = agent;
                }
                agent.State = possibleStates[_rnd.Next(possibleStates.Length)];
            }
        }
    }

    public void Update()
    {
        int[,] swap = new int[Agents.GetLength(0), Agents.GetLength(1)];

        for (int x = 0; x < Agents.GetLength(0); x++)
        {
            for (int y = 0; y < Agents.GetLength(1); y++)
            {
                TAgent orig = Agents[x, y];
                int[] neighbours = GetNeighbours(x, y);
                int nextState = orig.GetNextState(neighbours);
                swap[x, y] = nextState;
            }
        }

        for (int x = 0; x < Agents.GetLength(0); x++)
        {
            for (int y = 0; y < Agents.GetLength(1); y++)
            {
                Agents[x, y].State = swap[x, y];
            }
        }
    }

    public int[] GetNeighbours(int x, int y)
    {
        int[] neighbours = new int[9];

        for (int i = 0; i <= 2; i++)
        {
            int currX = x + i - 1;
            if (currX < 0 || currX >= Agents.GetLength(0))
                continue;

            for (int j = 0; j <= 2; j++)
            {
                int currY = y + j - 1;
                if (currY < 0 || currY >= Agents.GetLength(1))
                    continue;

                neighbours[j * 3 + i] = Agents[currX, currY].State;
            }
        }

        return neighbours;
    }

    public void Draw()
    {
        string output = "";

        for (int y = 0; y < Agents.GetLength(1); y++)
        {
            for (int x = 0; x < Agents.GetLength(0); x++)
            {
                var agent = Agents[x, y];
                output += (agent is not null ? agent.State : "-");
            }
            output += "\n";
        }

        Console.Clear();
        Console.CursorTop = 0;
        Console.CursorLeft = 0;
        Console.WriteLine(output);
    }
}
