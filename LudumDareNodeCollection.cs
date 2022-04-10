namespace LudumDareTools;

public class LudumDareNodeCollection<T> where T : LudumDareNode
{
    List<T> items;
    public List<T> Load()
    {
        if (items is null)
        {
            items = new();

            var path = Path.Combine("data", typeof(T).Name.ToLower());
            Directory.CreateDirectory(path);

            foreach (var file in Directory.EnumerateFiles(path, "*.json"))
            {
                var item = LudumDareNode.Load<T>(new FileInfo(file));
                if (item is not null)
                    items.Add(item);
            }
        }

        return items;
    }

    public void Save(T item)
    {
        var path = Path.Combine("data", typeof(T).Name.ToLower());

        Directory.CreateDirectory(path);

        path = Path.Combine(path, $"{item.id}.json");

        var json = Json.SerializeObject(item, Utils.GetJsonSettings());

        item.info = new(path);
        item.info.WriteAllText(json);

        items.Add(item);
    }

    public async Task<T> Get(long id)
    {
        var nodes = Load();

        if (!nodes.Any(x => x.id == id))
            return await RequestAndSave(id);

        var existingNode = nodes.FirstOrDefault(x => x.id == id);

        if (!existingNode.HasExpired())
        {
            await existingNode.BeforeReturn();
            return existingNode;
        }

        nodes.RemoveAll(x => x.id == id);
        return await RequestAndSave(id);
    }

    public async Task<T> RequestAndSave(long id)
    {
        using var httpClient = Utils.CreateHttpClient();

        var data = (await httpClient.Get($"node/get/{id}")).data.node[0];

        var node = (data as object).As<T>();

        try { node.BeforeSave(data); } catch { }

        Save(node);

        await node.BeforeReturn();

        return node;
    }
}