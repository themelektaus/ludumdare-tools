namespace LudumDareTools;

public class LudumDare
{
    public long id;
    public int number;

    public async Task<List<LD_Grade>> GetMyGrades(string token)
    {
        using var httpClient = Utils.CreateHttpClient(token);

        var (data, ex) = await httpClient.Get($"grade/getallmy/{id}");

        if (ex is not null || data is null)
            return null;

        var grades = (data.grade as List<object>)
                .Select(x => x.As<LD_Grade>())
                .ToList();

        return grades;
    }

    public async Task<List<LD_Comment>> GetMyComments(string token)
    {
        var comments = new List<LD_Comment>();

        using var httpClient = Utils.CreateHttpClient(token);

        var (data, ex) = await httpClient.Get($"comment/getmylistbyparentnode/{id}");

        var idList = ex is null && data is not null
            ? (data.comment as List<object>).As<List<long>>()
            : new List<long>();

        while (idList.Count > 0)
        {
            var _ids = string.Join('+', idList.Take(25));
            idList.RemoveRange(0, Math.Min(idList.Count, 25));

            (data, ex) = await httpClient.Get($"comment/get/{_ids}");
            if (ex is null && data is not null)
                comments.AddRange((data.comment as List<object>).Select(x => x.As<LD_Comment>()));
        }

        return comments;
    }
}