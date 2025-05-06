using System.Runtime.InteropServices;
namespace SimpleAgentModel;

public struct ArgsInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Path { get; set; }
    public bool StartLine { get; set; }
    public int NESW { get; set; }
    public int State { get; set; }
    public bool MultipleRun { get; set; }
    public int RunsCount { get; set; }
    public bool Draw { get; set; }
}

public class App
{
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AnsiHelper.Enable();

        ArgsInfo data = ParseArgs(args);

        if (!data.MultipleRun)
        {
            var model = RunModel(data);
            model.WriteAllModelInfo();
            model.History.PrintHistory();
        }
    }

    public static ArgsInfo ParseArgs(string[] args)
    {
        ArgsInfo res = new() { Draw = true };

        for (int i = 0; i < args.Length; i++)
        {
            var argLine = args[i];
            var arg = argLine.Split(":");

            switch (arg[0].ToLower())
            {
                case "x":
                    int.TryParse(arg[1], out int x);
                    res.X = x;
                    break;
                case "y":
                    int.TryParse(arg[1], out int y);
                    res.Y = y;
                    break;
                case "path":
                    res.Path = arg[1];
                    break;
                case "line":
                    bool.TryParse(arg[1], out bool line);
                    res.StartLine = line;
                    break;
                case "linedir":
                    res.StartLine = true;
                    int.TryParse(arg[1], out int nesw);
                    res.NESW = nesw;
                    break;
                case "linestate":
                    res.StartLine = true;
                    int.TryParse(arg[1], out int state);
                    res.State = state;
                    break;
                case "mult":
                    bool.TryParse(arg[1], out bool mult);
                    res.MultipleRun = mult;
                    break;
                case "multcount":
                    int.TryParse(arg[1], out int runcount);
                    res.RunsCount = runcount;
                    break;
                case "draw":
                    bool.TryParse(arg[1], out bool draw);
                    res.Draw = draw;
                    break;
                default:
                    Console.WriteLine($"Unknown argument: {argLine}");
                    break;
            }
        }

        if (res.X <= 0 || res.Y <= 0)
            throw new ApplicationException($"Map dimension must be greater than 0. Dimension: {res.X}x{res.Y}");
        if (res.MultipleRun && res.RunsCount <= 0)
            throw new ApplicationException($"The model must run at least once, you entered: {res.RunsCount}");
        if (res.StartLine && (res.NESW < 0 || res.NESW > 3))
            throw new ApplicationException($"The line must start on one edge, 0,1,2,3 - you entered {res.NESW}");

        return res;
    }

    public static Model RunModel(ArgsInfo data)
    {
        bool wait = true;
        var model = new Model(data.X, data.Y, data.Path);

        model.Randomize();
        if (data.StartLine)
            model.StartLine(data.NESW, data.State);
        model.Draw();
        string? inp;

        while (!model.ShouldEnd)
        {
            if (wait && data.Draw)
            {
                inp = Console.ReadLine();
                if (inp is not null && inp.StartsWith("c"))
                    wait = false;
            }

            model.Update();
            if (data.Draw)
                model.Draw();
        }

        return model;
    }
}
