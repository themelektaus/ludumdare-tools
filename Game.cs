namespace LudumDareTools;

public class Game : LudumDareNode
{
    public struct Magic
    {
        public float cool;
        public int feedback;
        public float given;
        public float grade;
        public float smart;
    }

    public long author;
    public string subtype;
    public string subsubtype;
    public int comments;
    public Magic magic;
    public string cover;

    [JsonIgnore]
    public Rating rating;

    [JsonIgnore]
    public User user;

    [JsonIgnore]
    public string static_cover =>
        cover?.Replace("///content", "https://static.jam.vg/content") + ".fit.jpg" ?? "";

    public override void BeforeSave(dynamic data)
    {
        cover = data.meta.cover;
    }

    public override async Task BeforeReturn()
    {
        user = await Utils.users.Get(author);
    }
}