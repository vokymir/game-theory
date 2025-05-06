using System.Runtime.InteropServices;
namespace SimpleAgentModel;

public class App
{
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AnsiHelper.Enable();

        var model = new Model(50, 15, "./Agents/Forest.agent");

        model.Randomize();
        model.Draw();

        while (!model.ShouldEnd)
        {
            Thread.Sleep(500);
            model.Update();
            model.Draw();
        }

        model.WriteAllModelInfo();
    }
}
