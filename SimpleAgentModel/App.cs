using System.Runtime.InteropServices;
namespace SimpleAgentModel;

public class App
{
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AnsiHelper.Enable();

        ArgsInfo data = ParseArgs(args);

        if (!data.ShouldContinue)
            return;

        if (data.MultipleRun)
        {
            RunModels(data);
        }
        else
        {
            var model = RunModel(data);
            model.WriteAllModelInfo();
            model.History.PrintHistory();
        }
    }

    public static void PrintHelp()
    {
        string output = @"
To use this program, give it parameters divided by spaces, in the form of <name>:<value>.
Example:
    dotnet run x:20 y:10 path:./Agents/Forest.agent

Available parameters:
    x           Int, setup map X dimension.
    y           Int, setup map Y dimension.
    path        String, path where find the agent.

    mult        Bool, wheter to run multiple simulations.
                Default: false
    multCount   Int, how many simulations.
    multLimit   Int, if the simulation is longer than limit, it will be ended. Any negative number and zero means unlimited.

    draw        Bool, if should draw the visualisation of the map.
                Default: true
    drawColors  Bool, if should use colors instead of state numbers when drawing.
                Default: true
    drawOver    Bool, if should draw each iteration of map in the same place, drawing over the old map.
                Default: false

    line        Bool, wheter to create a line of the same states on one edge of map.
                Default: false
    lineDir     Int, 0 = North, 1 = East, 2 = South, 3 = West. On which edge is the line.
    lineState   Int, which state the line should be at.
            ";
        Console.WriteLine(output);
    }

    public static ArgsInfo ParseArgs(string[] args)
    {
        ArgsInfo res = new() { MultipleRun = false, StartLine = false, Draw = true, DrawColors = true, DrawOver = false, ShouldContinue = true };

        if (args.Length <= 0)
            args = ["h"];

        for (int i = 0; i < args.Length; i++)
        {
            var argLine = args[i];
            var arg = argLine.Split(":");

            switch (arg[0].ToLower())
            {
                case "help":
                case "-h":
                case "h":
                case "--help":
                    PrintHelp();
                    res.ShouldContinue = false;
                    return res;
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
                case "multlimit":
                    int.TryParse(arg[1], out int multlimit);
                    res.MultipleRunsLimit = multlimit;
                    break;
                case "draw":
                    bool.TryParse(arg[1], out bool draw);
                    res.Draw = draw;
                    break;
                case "drawcolors":
                    bool.TryParse(arg[1], out bool drclrs);
                    res.DrawColors = drclrs;
                    break;
                case "drawover":
                    bool.TryParse(arg[1], out bool drover);
                    res.DrawOver = drover;
                    break;
                default:
                    Console.WriteLine($"Unknown argument: {argLine}\nTo see all available arguments, do: dotnet run --help");
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

    public static void RunModels(ArgsInfo data)
    {
        ModelRunInfo[] infos = new ModelRunInfo[data.RunsCount];

        ArgsInfo runData = data;
        runData.Draw = false;
        if (runData.MultipleRunsLimit <= 0)
            runData.MultipleRunsLimit = 1000;

        for (int i = 0; i < data.RunsCount; i++)
        {
            var model = RunModel(runData);
            infos[i] = model.GetAllModelInfo();

            Console.CursorLeft = 0;
            if (i > 0)
                Console.CursorTop = Console.CursorTop - 1;
            Console.WriteLine($"Processing {i + 1}/{data.RunsCount} simulations.");
        }
    }

    public static Model RunModel(ArgsInfo data)
    {
        var model = new Model(data.X, data.Y, data.Path);
        int skipIterations = 0;
        if (data.Draw)
            Console.WriteLine(@"
Hit enter to continue to the next iteration.
Instead write 's<int>' to skip <int> iterations.
");

        model.Randomize();
        if (data.StartLine)
            model.StartLine(data.NESW, data.State);
        if (data.Draw)
            model.Draw();

        string? inp;

        while (!model.ShouldEnd)
        {
            if (data.Draw && skipIterations <= 0)
            {
                inp = Console.ReadLine();
                if (inp is not null && inp.StartsWith("s"))
                {
                    int.TryParse(inp.Substring(1), out int count);
                    skipIterations = count;
                }
            }

            model.Update();
            if (data.Draw)
                model.Draw(data.DrawColors, !data.DrawOver);

            skipIterations--;
        }

        return model;
    }
}
