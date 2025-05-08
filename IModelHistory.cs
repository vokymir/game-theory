namespace SimpleAgentModel;

public interface IModelHistory
{
    public void Initialize(Model model);

    public void StateChanged(int round, int before, int after);

    public void PrintHistory();

    // number of changes through all history
    public int[] ChangesCount();
}
