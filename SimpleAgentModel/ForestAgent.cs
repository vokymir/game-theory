namespace SimpleAgentModel;
using System.Linq;

public class ForestAgent : Agent
{
    public override int GetNextState(int[] neighbours)
    {
        if (State != 2 && State != 1) // tree OR fire
            return State;
        if (State == 1) // burned
            return 0;

        var counts = neighbours.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        if (counts.ContainsKey(1) && counts[1] >= 1) { return 1; } // catch on fire

        return 2; // not burning
    }

    protected override void FillPossibleStates()
    {
        PossibleStates = [
            0, // burned
            1, // on fire
            2, // tree
            7 // not-tree
        ];
    }
}
