namespace SimpleAgentModel;

public struct ModelRunInfo
{
    public bool EndedNaturally { get; set; }
    public int Iterations { get; set; }
    // How many agents with given state exists at the end of run
    public Dictionary<int, int> FinalOccurrences { get; set; }
    public (int X, int Y) Dimensions { get; set; }
    public int[] PossibleStates { get; set; }
    public IModelHistory History { get; set; }
}
