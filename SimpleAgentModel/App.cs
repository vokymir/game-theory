using System.Runtime.InteropServices;
namespace SimpleAgentModel;

public class App
{
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AnsiHelper.Enable();

        RunModel();
    }

    public static void RunModel()
    {
        bool wait = true;
        var model = new Model(50, 15, "./Agents/Forest.agent");

        model.Randomize();
        model.Draw();
        string? inp;

        while (!model.ShouldEnd)
        {
            if (wait)
            {
                inp = Console.ReadLine();
                if (inp is not null && inp.StartsWith("c"))
                    wait = false;
            }

            model.Update();
            model.Draw();
        }

        model.WriteAllModelInfo();
    }
}
