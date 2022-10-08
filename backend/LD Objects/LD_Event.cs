namespace LudumDareTools;

public class LD_Event : LD_Object
{
    public struct Feed
    {
        public long id;
    }

    public List<Feed> feeds = new();
}