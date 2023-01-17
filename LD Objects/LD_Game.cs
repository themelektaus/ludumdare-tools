namespace LudumDareTools;

public class LD_Game : LD_Node
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
    public LD_Rating rating;

    [JsonIgnore]
    public float averageRating =>
        rating.GetTotal() / opt_outs.Where(x => !x).Count();

    [JsonIgnore]
    public float rateProgress =>
        rating.GetRatedCount(opt_outs) / opt_outs.Where(x => !x).Count();

    [JsonIgnore]
    public LD_User user;

    [JsonIgnore]
    public int userComments;

    [JsonIgnore]
    public string static_cover
    {
        get
        {
            if (cover is null)
                return null;

            return cover.Replace("///content", "https://static.jam.vg/content") + ".jpg";
        }
    }

    [JsonIgnore]
    public string thumbnail_url => $"/api/thumbnail/{id}";

    [JsonIgnore]
    public string gif_url
    {
        get
        {
            if (static_gifs.Count > 0)
                return $"/api/images/{id}/gif";

            return thumbnail_url;
        }
    }

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

}