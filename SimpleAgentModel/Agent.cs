
namespace SimpleAgentModel;

/// <summary>
/// One agent, have state and properties.
/// </summary>
public class Agent
{
    public int State { get; set; } = 0;
    public static int[] PossibleStates { get; protected set; } = Array.Empty<int>();
    protected static float[] _statesProbabilities = Array.Empty<float>();
    public static float[] StatesProbabilities
    {
        get => _statesProbabilities;
        protected set
        {
            var sum = value.Sum();
            if (Math.Abs(1 - sum) > 0.01)
                throw new ApplicationException("The sum of probabilities must be 1.");
            _statesProbabilities = value;
        }
    }

    public Agent() { FillPossibleStates(); }

    virtual public int GetNextState(int[] neighbours)
    {
        return State;
    }

    virtual protected void FillPossibleStates()
    {
        PossibleStates = [0, 1, 2, 3, 4, 5, 6, 7];
        StatesProbabilities = [0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f];
    }

    public int[] GetPossibleStates()
    {
        return PossibleStates;
    }
}
