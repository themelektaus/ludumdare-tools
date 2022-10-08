namespace LudumDareTools;

public class LD_EventCollection : LD_ObjectCollection<LD_Event>
{
    protected override async Task Request(List<long> idList, List<object> dataList)
    {
        if (idList.Count == 0)
            return;

        var ldId = idList.Single();

        var feeds = new List<object>();

        using var httpClient = Utils.CreateHttpClient();

        for (int i = 0; ; i++)
        {
            var limit = 50;
            var offset = limit * i;
            var url = $"node/feed/{ldId}/smart+reverse+parent/item/game/compo+jam/?limit={limit}&offset={offset}";

            List<dynamic> list = (await httpClient.Get(url)).data.feed;
            if (list.Count == 0)
                break;

            foreach (var feed in list)
                feeds.Add(feed);
        }

        dataList.Add(new { id = ldId, feeds });
    }
}