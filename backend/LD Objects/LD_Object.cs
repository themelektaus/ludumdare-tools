namespace LudumDareTools;

public abstract class LD_Object
{
    public static T Load<T>(FileInfo info) where T : LD_Object
    {
        var json = File.ReadAllText(info.FullName);

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

    [JsonIgnore]
    [FullyIgnore]
    public FileInfo info;

    public long id;

    public bool HasExpired()
    {
        var age = DateTime.Now - info.LastWriteTime;
        return age.TotalHours >= 24;
    }

    public virtual void BeforeSave(dynamic data)
    {

    }

    public virtual Task BeforeReturn()
    {
        return Task.CompletedTask;
    }
}