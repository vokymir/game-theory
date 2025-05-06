using System.Runtime.InteropServices;
namespace SimpleAgentModel;

public class App
{
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AnsiHelper.Enable();


        var model = new Model(30, 5, "./Agents/Forest.agent");

        model.RandomizeGrid(ForestAgent.PossibleStates);
        model.Draw();

        while (true)
        {
            Thread.Sleep(1000);
            model.Update();
            model.Draw();
        }
    }
}
