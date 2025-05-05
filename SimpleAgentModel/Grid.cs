
namespace SimpleAgentModel;

/// <summary>
/// Store grid of agents,
/// show it in the console.
/// </summary>
public class Grid<TAgent> where TAgent : Agent
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

        for (int i = -1; i < 2; i++)
        {
            int currX = x + i;
            if (currX < 0 || currX >= Agents.GetLength(0))
                continue;

            for (int j = -1; j < 2; j++)
            {
                int currY = y + j;
                if (currY < 0 || currY >= Agents.GetLength(1))
                    continue;

                neighbours[j * 3 + i] = Agents[currX, currY].State;
            }
        }

        return neighbours;
    }

    public void Draw()
    {

    }
}
