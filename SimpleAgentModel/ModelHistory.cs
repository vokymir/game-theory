namespace SimpleAgentModel;

public class ModelHistory : IModelHistory
{
    private List<int[]> _history = new();
    private int[] _states = [];
    private Dictionary<int, int> _state2index = new();

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

    private void AddRound()
    {
        _history.Add(new int[_states.Length * 2]);
    }
}
