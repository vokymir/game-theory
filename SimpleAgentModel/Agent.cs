
namespace SimpleAgentModel;

/// <summary>
/// One agent, have state and properties.
/// </summary>
public class Agent
{
    public int State { get; set; }

    virtual public int GetNextState(int[] neighbours)
    {
        return State;
    }
}
