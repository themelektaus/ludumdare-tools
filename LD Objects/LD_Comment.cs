namespace LudumDareTools;

public struct LD_Comment
{
    public long id;
    public long parent;
    public long node;
    public long supernode;
    public DateTime created;
    public DateTime modified;
    public long version;
    public int flags;
    public string body;
    public int love;

    [JsonProperty("love-timestamp")]
    public DateTime loveTimestamp;
}