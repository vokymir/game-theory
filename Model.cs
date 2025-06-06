using System.Text;
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
    public bool ShouldEnd { get; set; } = false;
    public int Iteration { get; private set; } = 0;
    public int[] PossibleStates { get => ExemplarAgent.GetPossibleStates(); }
    public IModelHistory History = new ModelHistory();

    public Model(int x, int y, string path)
    {
        AgentGenerator = new AgentLoader() { Path = path };
        Agents = new Agent[x, y];
        ExemplarAgent = AgentGenerator.Create();
        History.Initialize(this);
    }

    public Model(int x, int y, Type t)
    {
        AgentGenerator = new AgentLoader(t);
        Agents = new Agent[x, y];
        ExemplarAgent = AgentGenerator.Create();
        History.Initialize(this);
    }

    public void Randomize()
    {
        int[] possibleStates = ExemplarAgent.GetPossibleStates();

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
                agent.State = agent.Probability2State(_rnd.NextSingle());

                History.StateChanged(Iteration, -1, agent.State);
            }
        }
    }

    public void StartLine(int NESW, int state)
    {
        int x = 0;
        int y = 0;

        if (NESW == 0 || NESW == 2)
        {
            y = NESW == 0 ? 0 : Agents.GetLength(1) - 1;
            for (x = 0; x < Agents.GetLength(0); x++)
                Agents[x, y].State = state;
        }
        else
        {
            x = NESW == 1 ? Agents.GetLength(0) - 1 : 0;
            for (y = 0; y < Agents.GetLength(1); y++)
                Agents[x, y].State = state;
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
        var changesCount = History.ChangesCount();
        string output = $"{State2Color.Reset()}Iteration: {Iteration}";

        Console.WriteLine(output);
    }

    public ModelRunInfo GetAllModelInfo()
    {
        var counts = CountAgentsByState();
        int rounds = Iteration;
        var changes = History.ChangesCount();

        ModelRunInfo info = new()
        {
            Iterations = rounds,
            FinalOccurrences = counts,
            Dimensions = (Agents.GetLength(0), Agents.GetLength(1)),
            PossibleStates = PossibleStates,
            History = History,
            EndedNaturally = changes[changes.Length - 1] == 0
        };

        return info;
    }

    private Dictionary<int, int> CountAgentsByState()
    {
        var counts = new Dictionary<int, int>();

        foreach (var state in PossibleStates)
            counts[state] = 0;

        for (int x = 0; x < Agents.GetLength(0); x++)
        {
            for (int y = 0; y < Agents.GetLength(1); y++)
            {
                var agent = Agents[x, y];
                if (agent == null)
                    continue;

                int st = agent.State;
                counts[st]++;
            }
        }

        return counts;
    }

    public static void WriteAllModelInfo(ModelRunInfo info)
    {
        var build = new StringBuilder();
        foreach (int state in info.PossibleStates)
            build.AppendLine($"{State2Color.Background(state)}State {state}{State2Color.Reset()}:\t{info.FinalOccurrences[state]}");

        string output = $@"
Dimensions: {info.Dimensions.X}x{info.Dimensions.Y}
Agent counts at the end:
{build.ToString()}
Simulation ended naturally: {info.History.ChangesCount()[info.History.ChangesCount().Length - 1] == 0}";

        Console.WriteLine(output);
    }

    public static void WriteAllModelsInfo(ModelRunInfo[] infos)
    {
        int simulationsCount = infos.Length;
        int[] possibleStates = infos[0].PossibleStates;
        var endedNaturally = infos.Sum((ModelRunInfo m) => m.EndedNaturally == true ? 1 : 0);
        Dictionary<int, int[]> endStates = new();
        StringBuilder sb = new();

        for (int i = 0; i < simulationsCount; i++)
        {
            var info = infos[i];
            foreach (var stateKey in info.FinalOccurrences.Keys)
            {
                if (!endStates.ContainsKey(stateKey))
                    endStates[stateKey] = new int[simulationsCount];
                endStates[stateKey][i] += info.FinalOccurrences[stateKey];
            }
        }


        sb.AppendLine($"Simulations run: {simulationsCount}");
        sb.AppendLine("How many states were 'alive' at the end.");
        sb.AppendLine("State\tSum\tAvg\tMed");

        foreach (var endState in endStates.Keys)
            sb.AppendLine($"{State2Color.Foreground(endState)}{endState}{State2Color.Reset()}\t{endStates[endState].Sum().ToString()}\t{endStates[endState].Average().ToString("0.00")}\t{endStates[endState][simulationsCount / 2]}");
        sb.AppendLine($"Ended naturally: {endedNaturally}\nWhich is {(endedNaturally / (float)simulationsCount * 100).ToString("0.00")}%");

        Console.WriteLine(sb.ToString());
    }
}
