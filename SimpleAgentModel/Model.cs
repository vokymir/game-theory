
namespace SimpleAgentModel;

/// <summary>
/// Load info from file.
/// Have grid full of agents.
/// Run the simulation.
/// </summary>
public class Model
{
    public Agent[,] Agents { get; set; }
    private Random _rnd = new Random();
    protected Agent ExemplarAgent = new();
    protected AgentLoader AgentGenerator;
    public List<int> ChangesCountHistory { get; } = new();
    public int LastChangesCount { get => ChangesCountHistory.LastOrDefault(-1); }
    public bool ShouldEnd { get; private set; } = false;
    public int Iteration { get; private set; } = 0;
    public int[] PossibleStates { get => ExemplarAgent.GetPossibleStates(); }
    public IModelHistory History = new ModelHistory();

    public Model(int x, int y, string path)
    {
        AgentGenerator = new AgentLoader() { Path = path };
        Agents = new Agent[x, y];
        History.Initialize(this);
    }

    public Model(int x, int y, Type t)
    {
        AgentGenerator = new AgentLoader(t);
        Agents = new Agent[x, y];
    }

    public void Randomize()
    {
        int[] possibleStates = AgentGenerator.Create().GetPossibleStates();

        for (int x = 0; x < Agents.GetLength(0); x++)
        {
            for (int y = 0; y < Agents.GetLength(1); y++)
            {
                var agent = Agents[x, y];
                if (agent is null)
                {
                    agent = AgentGenerator.Create();
                    Agents[x, y] = agent;
                }
                agent.State = possibleStates[_rnd.Next(possibleStates.Length)];

                History.StateChanged(Iteration, -1, agent.State);
            }
        }
    }

    public void Update()
    {
        int[,] swap = new int[Agents.GetLength(0), Agents.GetLength(1)];
        int changesCount = 0;
        Iteration += 1;

        for (int x = 0; x < Agents.GetLength(0); x++)
        {
            for (int y = 0; y < Agents.GetLength(1); y++)
            {
                Agent orig = Agents[x, y];
                int[] neighbours = GetNeighbours(x, y);
                int nextState = orig.GetNextState(neighbours);
                swap[x, y] = nextState;
                History.StateChanged(Iteration, orig.State, nextState);
                if (orig.State != nextState) changesCount++;
            }
        }

        if (changesCount == 0)
            ShouldEnd = true;
        ChangesCountHistory.Add(changesCount);

        for (int x = 0; x < Agents.GetLength(0); x++)
        {
            for (int y = 0; y < Agents.GetLength(1); y++)
            {
                Agents[x, y].State = swap[x, y];
            }
        }
    }

    private int[] GetNeighbours(int x, int y)
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

    public void Draw(bool useColors = true, bool seeHistory = true)
    {
        if (useColors) DrawWithColors(seeHistory);
        else DrawWithNumbers(seeHistory);

        WriteInfo();

        if (seeHistory)
            Console.WriteLine("-----");
    }

    private void DrawWithColors(bool seeHistory)
    {
        string output = "";

        for (int y = 0; y < Agents.GetLength(1); y++)
        {
            for (int x = 0; x < Agents.GetLength(0); x++)
            {
                var agent = Agents[x, y];
                string add;
                if (agent is null)
                    add = State2Color.Reset() + "XX";
                else
                    add = State2Color.Background(agent.State) + "  " + State2Color.Reset();

                output += add;
            }
            if (y < Agents.GetLength(1) - 1)
                output += "\n";
        }

        if (!seeHistory)
        {
            Console.Clear();
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
        }
        Console.WriteLine(output);
    }

    private void DrawWithNumbers(bool seeHistory)
    {
        string output = "";

        for (int y = 0; y < Agents.GetLength(1); y++)
        {
            for (int x = 0; x < Agents.GetLength(0); x++)
            {
                var agent = Agents[x, y];
                output += (agent is not null ? agent.State : "-");
            }

            if (y < Agents.GetLength(1) - 1)
                output += "\n";
        }

        if (!seeHistory)
        {
            Console.Clear();
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
        }
        Console.WriteLine(output);
    }

    private void WriteInfo()
    {
        string output = $"{State2Color.Background(0)}Iteration: {Iteration}\nChanges count: {LastChangesCount}";

        Console.WriteLine(output);
    }

    public void WriteAllModelInfo()
    {
        string progress = "";
        for (int i = 0; i < Iteration - 1; i++)
        {
            progress += $"{(i + 1).ToString("D" + (int)Math.Ceiling(Math.Log10(ChangesCountHistory.Count)))} - {ChangesCountHistory[i].ToString("D" + (int)Math.Ceiling(Math.Log10(Agents.Length)))}: {new string('x', Math.Max(0, (int)Math.Ceiling(Math.Log2(ChangesCountHistory[i]))))}\n";
        }

        string output = $@"
Dimensions: {Agents.GetLength(0)}x{Agents.GetLength(1)}
Graph of changes over time:
{progress}
            ";
        Console.WriteLine(output);
    }
}
