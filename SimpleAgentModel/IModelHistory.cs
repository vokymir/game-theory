namespace SimpleAgentModel;

public interface IModelHistory
{
    public void Initialize(Model model);

    public void StateChanged(int round, int before, int after);

}
