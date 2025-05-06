using System.Runtime.InteropServices;
namespace SimpleAgentModel;

public class App
{
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AnsiHelper.Enable();

        var data = ParseArgs(args);

        if (!data.MultipleRun)
            RunModel(data.X, data.Y, data.Path, true);
    }

    public static (int X, int Y, string Path, bool MultipleRun, int RunsCount) ParseArgs(string[] args)
    {
        int x, y, runsCount = 0;
        string path;
        bool multiple;
        int.TryParse(args[0], out x);
        int.TryParse(args[1], out y);
        path = args[2];
        multiple = args[3].StartsWith("1") || args[3].ToLower().StartsWith("true");
        if (multiple)
            int.TryParse(args[4], out runsCount);

        if (x <= 0 || y <= 0 || (multiple && runsCount <= 0))
            throw new ApplicationException("Invalid parameters from console.");

        return (x, y, path, multiple, runsCount);
    }

    public static void RunModel(int x, int y, string path, bool shouldDraw)
    {
        bool wait = true;
        var model = new Model(x, y, path);

        model.Randomize();
        model.Draw();
        string? inp;

        while (!model.ShouldEnd)
        {
            if (wait && shouldDraw)
            {
                inp = Console.ReadLine();
                if (inp is not null && inp.StartsWith("c"))
                    wait = false;
            }

            model.Update();
            if (shouldDraw)
                model.Draw();
        }

        model.WriteAllModelInfo();
    }
}
