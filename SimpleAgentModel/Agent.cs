namespace SimpleAgentModel;

/// <summary>
/// One agent, have state and properties.
/// </summary>
public class Agent
{
    public int State { get; set; } = 0;
    public static int[] PossibleStates { get; protected set; } = Array.Empty<int>();
    protected static float[] _statesProbabilities = Array.Empty<float>();
    protected static float[] _statesProbabilitiesSum = Array.Empty<float>();
    public static float[] StatesProbabilities
    {
        get => _statesProbabilities;
        protected set
        {
            if (value.Length != PossibleStates.Length)
                throw new ApplicationException("The length of probabilities must be the same as the length of possible states.");
            float sum = 0;
            _statesProbabilitiesSum = new float[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                sum += value[i];
                _statesProbabilitiesSum[i] = sum;
            }
            if (Math.Abs(1f - sum) > 0.1)
                throw new ApplicationException("The sum of probabilities must be 1.");
            _statesProbabilities = value;
        }
    }

    public Agent() { FillPossibleStates(); }

    virtual public int GetNextState(int[] neighbours) => State;

    virtual protected void FillPossibleStates()
    {
        PossibleStates = [0, 1, 2, 3, 4, 5, 6, 7];
        StatesProbabilities = [0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f];
    }

    public int[] GetPossibleStates() => PossibleStates;
    public float[] GetStatesProbabilities() => StatesProbabilities;
    public int Probability2State(float probability)
    {
        int idx = 0;
        while (_statesProbabilitiesSum[idx] < probability) idx++;
        return PossibleStates[idx];
    }
}
