
namespace SimpleAgentModel;

/// <summary>
/// Load info from file.
/// Have grid full of agents.
/// Run the simulation.
/// </summary>
public class Model<TAgent> where TAgent : Agent, new()
{
    public Grid<TAgent> AgentGrid { get; set; }

    public Model(int x, int y)
    {
        AgentGrid = new Grid<TAgent>(x, y);
    }
}
