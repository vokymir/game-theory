namespace SimpleAgentModel;

public class State2Color
{
    public static string Foreground(int i)
    {
        if (i < 0 || i > 7)
            return string.Empty;
        else
            return $"\x1b[3{i}m";
    }

    public static string Background(int i)
    {
        if (i < 0 || i > 7)
            return string.Empty;
        else
            return $"\x1b[4{i}m";
    }

    public static string Reset() => "\x1b[0m";
}
