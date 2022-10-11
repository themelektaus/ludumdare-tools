namespace LudumDareTools;

public static class Cache
{
    public static readonly List<LudumDare> ludumDareList = new();

    public static readonly LD_NodeCollection<LD_Game> games = new();
    public static readonly LD_NodeCollection<LD_User> users = new();
    public static readonly LD_EventCollection events = new();
}