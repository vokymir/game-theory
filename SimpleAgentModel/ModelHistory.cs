namespace SimpleAgentModel;

public class ModelHistory : IModelHistory
{
    private List<int[]> _history = new();
    private int[] _states = [];
    private Dictionary<int, int> _state2index = new();
    private int[] _changesCount = Array.Empty<int>();

    public void Initialize(Model model)
    {
        _states = model.PossibleStates;
        Array.Sort(_states);
        for (int i = 0; i < _states.Length; i++)
        {
            _state2index[_states[i]] = i;
        }
    }

    public void StateChanged(int round, int before, int after)
    {
        while (round >= _history.Count)
            AddRound();

        if (before == after)
            return;

        if (_state2index.ContainsKey(before))
        {
            int beforeIdx = _state2index[before];
            _history[round][2 * beforeIdx + 1] -= 1;
        }

        if (_state2index.ContainsKey(after))
        {
            int afterIdx = _state2index[after];
            _history[round][2 * afterIdx] += 1;
        }
    }

    public void PrintHistory()
    {
        CountChanges();

        // Header
        Console.Write("Round\t");
        for (int i = 0; i < _states.Length; i++)
        {
            Console.Write($"{State2Color.Background(_states[i])}{_states[i]}(+)\t{_states[i]}(-){State2Color.Reset()}\t");
        }
        Console.Write("Total changes\t");
        Console.WriteLine();

        // Rows
        for (int round = 0; round < _history.Count; round++)
        {
            Console.Write($"{round.ToString("D" + Math.Ceiling(Math.Log10(_history.Count)))}\t");
            var row = _history[round];
            for (int stateIdx = 0; stateIdx < _states.Length; stateIdx++)
            {
                int added = row[2 * stateIdx];
                int removed = -row[2 * stateIdx + 1];  // we stored removals as negative increments
                Console.Write($"{added}\t{removed}\t");
            }
            Console.Write($"{_changesCount[round]}\t");
            Console.WriteLine();
        }
    }

    private void CountChanges()
    {
        _changesCount = new int[_history.Count];

        for (int i = 0; i < _history.Count; i++)
        {
            var arr = _history[i];
            int changesCount = 0;
            foreach (int stateChanges in arr)
                if (stateChanges > 0) changesCount += stateChanges;

            _changesCount[i] = changesCount;
        }
    }

    private void AddRound()
    {
        _history.Add(new int[_states.Length * 2]);
    }
}
