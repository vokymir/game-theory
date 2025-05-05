
namespace SimpleAgentModel;

/// <summary>
/// One agent, have state and properties.
/// </summary>
public class Agent
{
    public int State { get; set; } = 0;
    public static int[] PossibleStates { get; protected set; }

    public Agent() { FillPossibleStates(); }

    virtual public int GetNextState(int[] neighbours)
    {
        return State;
    }

    virtual protected void FillPossibleStates()
    {
        PossibleStates = [0, 1, 2];
    }
}
