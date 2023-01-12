namespace LudumDareTools;

using LudumDareTools;

public abstract class LD_ObjectCollection<T> where T : LD_Object
{
    static readonly object _itemsHandle = new();

    Dictionary<long, T> _items;
    Dictionary<long, T> items
    {
        get
        {
            Dictionary<long, T> result;
            lock (_itemsHandle)
                result = _items;
            return result;
        }
        set
        {
            lock (_itemsHandle)
                _items = value;
        }
    }

    public Dictionary<long, T> Load()
    {
        if (items is null)
        {
            items = new();

            var path = Path.Combine("data", typeof(T).Name.ToLower());
            Directory.CreateDirectory(path);

            foreach (var file in Directory.EnumerateFiles(path, "*.json"))
            {
                var item = LD_Object.Load<T>(new FileInfo(file));
                if (item is not null)
                    items.Add(item.id, item);
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
        File.WriteAllText(item.info.FullName, json);

        items[item.id] = item;
    }

    public async Task<T> Get(long id, Dictionary<long, T> items = null)
    {
        items ??= Load();
        if (items.TryGetValue(id, out var item))
        {
            await item.BeforeReturn();
            return item;
        }
        return null;
    }

    public async Task<T> Get(long id)
    {
        return (await Get(new List<long>() { id })).FirstOrDefault();
    }

    public async Task<List<T>> Get(IEnumerable<long> ids)
    {
        var items = Load();
        var result = new List<T>();
        foreach (var id in ids)
        {
            var item = await Get(id, items);
            if (item is not null)
                result.Add(item);
        }
        return result;
    }

    public async Task Fetch(IEnumerable<long> ids)
    {
        var items = Load();
        var newIds = new List<long>();
        foreach (var id in ids)
        {
            var item = await Get(id, items);
            if (item is null || item.HasExpired())
                newIds.Add(id);
        }
        var fetchedItems = await RequestAndSave(newIds);
        foreach (var item in fetchedItems)
            items[item.id] = item;
    }

    async Task<List<T>> RequestAndSave(List<long> idList)
    {
        var dataList = new List<object>();

        await Request(idList, dataList);

        var result = new List<T>();

        foreach (var data in dataList)
        {
            var @object = data.As<T>();
            try { @object.BeforeSave(data); } catch { }
            Save(@object);
            await @object.BeforeReturn();
            result.Add(@object);
        }

        return result;
    }

    protected abstract Task Request(List<long> idList, List<object> dataList);
}