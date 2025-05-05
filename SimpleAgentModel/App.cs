using System.Runtime.InteropServices;
namespace SimpleAgentModel;

using AgentType = ForestAgent;

public class App
{
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AnsiHelper.Enable();

        var agent = new AgentType();

        var model = new Model<AgentType>(30, 5);

        model.AgentGrid.RandomizeGrid(ForestAgent.PossibleStates);
        model.AgentGrid.Draw();

        while (true)
        {
            Thread.Sleep(1000);
            model.AgentGrid.Update();
            model.AgentGrid.Draw();
        }
    }
}
