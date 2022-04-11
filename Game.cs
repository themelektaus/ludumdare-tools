namespace LudumDareTools;

public class Game : LudumDareNode
{
    public struct Meta
    {
        public string cover;

        [JsonProperty("grade-01-out")] public string grade01Out;
        [JsonProperty("grade-02-out")] public string grade02Out;
        [JsonProperty("grade-03-out")] public string grade03Out;
        [JsonProperty("grade-04-out")] public string grade04Out;
        [JsonProperty("grade-05-out")] public string grade05Out;
        [JsonProperty("grade-06-out")] public string grade06Out;
        [JsonProperty("grade-07-out")] public string grade07Out;
        [JsonProperty("grade-08-out")] public string grade08Out;
    }

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
    public Meta meta;
    public Magic magic;
    public string cover;
    public bool[] opt_outs = new bool[8];

    [JsonIgnore]
    public Rating rating;

    [JsonIgnore]
    public User user;

    [JsonIgnore]
    public int userComments;

    [JsonIgnore]
    public string static_cover =>
        cover?.Replace("///content", "https://static.jam.vg/content") + ".fit.jpg" ?? "";

    public override void BeforeSave(dynamic data)
    {
        cover = meta.cover;

        opt_outs[0] = meta.grade01Out == "1";
        opt_outs[1] = meta.grade02Out == "1";
        opt_outs[2] = meta.grade03Out == "1";
        opt_outs[3] = meta.grade04Out == "1";
        opt_outs[4] = meta.grade05Out == "1";
        opt_outs[5] = meta.grade06Out == "1";
        opt_outs[6] = meta.grade07Out == "1";
        opt_outs[7] = meta.grade08Out == "1";
    }

    public override async Task BeforeReturn()
    {
        user = await Utils.users.Get(author);
    }
}