namespace LudumDareTools;

public class LD_NodeCollection<T> : LD_ObjectCollection<T> where T : LD_Node
{
    protected override async Task Request(List<long> idList, List<object> dataList)
    {
        using var httpClient = Runtime.instance.CreateHttpClient();

        var MAX = Constants.LIMIT / 2;

        while (idList.Count > 0)
        {
            var _ids = string.Join('+', idList.Take(MAX));
            idList.RemoveRange(0, Math.Min(idList.Count, MAX));

            var response = await httpClient.Get($"node/get/{_ids}");
            foreach (var data in response.data.node)
                dataList.Add(data as object);
        }
    }
}