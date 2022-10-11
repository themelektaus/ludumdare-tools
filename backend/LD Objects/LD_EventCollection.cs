namespace LudumDareTools;

public class LD_EventCollection : LD_ObjectCollection<LD_Event>
{
    protected override async Task Request(List<long> idList, List<object> dataList)
    {
        if (idList.Count == 0)
            return;

        var ldId = idList.Single();

        var feeds = new List<object>();

        using var httpClient = Runtime.instance.CreateHttpClient();

        for (int i = 0; ; i++)
        {
            var offset = Constants.LIMIT * i;
            var url = $"node/feed/{ldId}/smart+reverse+parent/item/game/compo+jam/?limit={Constants.LIMIT}&offset={offset}";

            List<dynamic> list = (await httpClient.Get(url)).data.feed;
            if (list.Count == 0)
                break;

            foreach (var feed in list)
                feeds.Add(feed);
        }

        dataList.Add(new { id = ldId, feeds });
    }
}