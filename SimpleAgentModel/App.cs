
namespace SimpleAgentModel;

public class App
{
    public static void Main(string[] args)
    {
        var model = new Model<Agent>(30, 5);

        model.AgentGrid.Draw();

        model.AgentGrid.RandomizeGrid([0, 1, 2]);

        model.AgentGrid.Draw();

        while (true)
        {
            model.AgentGrid.Update();

            model.AgentGrid.Draw();

            Thread.Sleep(1000);
        }
    }
}
