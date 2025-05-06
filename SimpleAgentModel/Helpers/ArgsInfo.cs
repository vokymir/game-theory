namespace SimpleAgentModel;

public struct ArgsInfo
{
    public bool ShouldContinue { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string Path { get; set; }
    public bool StartLine { get; set; }
    public int NESW { get; set; }
    public int State { get; set; }
    public bool MultipleRun { get; set; }
    public int RunsCount { get; set; }
    public bool Draw { get; set; }
    public bool DrawColors { get; set; }
    public bool DrawOver { get; set; }
}
