namespace LudumDareTools;

public abstract class LudumDareNode
{
    public static T Load<T>(FileInfo info) where T : LudumDareNode
    {
        var json = info.ReadAllText();

        T node;
        try
        {
            node = Json.DeserializeObject<T>(json, Utils.GetJsonSettings());
        }
        catch
        {
            info.Delete();
            return default;
        }

        node.info = info;
        return node;
    }

    [FullyIgnore] public FileInfo info;

    public long id;
    public long parent;
    public long superparent;
    public string type;
    public DateTime published;
    public DateTime created;
    public DateTime modified;
    public long version;
    public string slug;
    public string name;
    public string body;
    public string path;

    [JsonIgnore]
    public string static_path => $"https://ldjam.com{path}";

    [JsonIgnore]
    public string static_body => body.Replace("///raw", "https://static.jam.vg/raw");

    public abstract void BeforeSave(dynamic data);
    public abstract Task BeforeReturn();

    public bool HasExpired()
    {
        var age = DateTime.Now - info.LastWriteTime;
        return age.TotalHours >= 24;
    }
}