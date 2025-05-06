namespace SimpleAgentModel;

public struct ModelRunInfo
{
    public bool Ended { get; set; }
    public int Iterations { get; set; }
    // How many agents with given state exists at the end of run
    public Dictionary<int, int> FinalOccurrences { get; set; }
}
