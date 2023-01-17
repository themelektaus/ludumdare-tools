namespace LudumDareTools;

using Microsoft.AspNetCore.Http;
using System.Dynamic;
using System.Net.Http;
using System.Text;

public static class ExtensionMethods
{
    public static async Task<ApiResult> Get(this HttpClient @this, string url)
    {
        try
        {
            Console.WriteLine($"GET {Constants.API_BASE_URL}{url}");
            string json = await @this.GetStringAsync(url);
            var result = Json.DeserializeObject<ExpandoObject>(json, Utils.GetJsonSettings());
            return new() { data = result };
        }
        catch (HttpRequestException ex)
        {
            return new() { exception = ex };
        }
    }

    public static async Task<HttpResponseMessage> Post(this HttpClient @this, string url, params (string key, object value)[] formData)
    {
        var value = formData.Select(x =>
        {
            string value;

            if (x.value is string stringValue)
                value = stringValue;
            else
                value = Encoding.UTF8.GetString((byte[]) x.value);

            return new[] { x.key, value };

        }).ToArray();

        var collection = value.Select(x => new KeyValuePair<string, string>(x[0], x[1]));
        var content = new FormUrlEncodedContent(collection);

        Console.WriteLine($"POST {Constants.API_BASE_URL}{url}");
        return await @this.PostAsync(url, content);
    }

    public static T As<T>(this object @this)
    {
        var json = Json.SerializeObject(@this, Utils.GetJsonSettings());
        return Json.DeserializeObject<T>(json, Utils.GetJsonSettings());
    }

    public static bool Has(this string @this, string value)
    {
        return @this.Contains(value, StringComparison.InvariantCultureIgnoreCase);
    }

    public static string GetFormString(this HttpContext @this, string key)
        => @this.GetFormValue(key, x => x);

    public static bool GetFormBoolean(this HttpContext @this, string key)
        => @this.GetFormValue(key, x => x.Equals("true", StringComparison.InvariantCultureIgnoreCase));

    public static int GetFormInteger(this HttpContext @this, string key)
        => @this.GetFormValue(key, x => int.TryParse(x, out var value) ? value : 0);

    public static long GetFormLong(this HttpContext @this, string key)
        => @this.GetFormValue(key, x => long.TryParse(x, out var value) ? value : 0);

    public static float GetFormFloat(this HttpContext @this, string key)
        => @this.GetFormValue(key, x => float.TryParse(x, out var value) ? value : 0);

    static T GetFormValue<T>(this HttpContext @this, string key, Func<string, T> getValue)
    {
        var request = @this.Request;
        if (request.HasFormContentType)
            return getValue((string) request.Form?[key] ?? null);
        return getValue(null);
    }

    static HttpClient CreateHttpClient(string token = null)
    {
        var httpClient = new HttpClient { BaseAddress = new(Constants.API_BASE_URL) };
        if (!string.IsNullOrEmpty(token))
            httpClient.DefaultRequestHeaders.Add("Cookie", Constants.TOKEN_PREFIX + token);
        return httpClient;
    }

    public static HttpClient CreateHttpClient(this Runtime _)
    {
        return CreateHttpClient();
    }

    public static HttpClient CreateHttpClient(this HttpContext @this)
    {
        var token = @this.GetFormString("token");
        return CreateHttpClient(token);
    }

    public static async Task Fetch(this Runtime @this, int number)
    {
        var ludumDare = await @this.GetLudumDare(number);
        await Cache.events.Fetch(new List<long>() { ludumDare.id });
        var @event = await ludumDare.GetEvent();
        await Cache.games.Fetch(@event.feeds.Select(x => x.id).Distinct());
        var games = await @event.GetGames();
        await Cache.users.Fetch(games.Select(x => x.author).Distinct());
    }

    public static async Task FetchComments(this Runtime @this, LD_Event @event)
    {
        var games = await @event.GetGames();
        foreach (var game in games)
        {
            // TODO: Fetch comments
            _ = $"https://api.ldjam.com/vx/comment/getbynode/{game.id}";
        }
    }

    public static async Task<LD_User> GetUser(this HttpContext @this)
    {
        using var httpClient = @this.CreateHttpClient();
        var result = await httpClient.Get($"user/get");
        return result.TryGet(x => (x.node as object).As<LD_User>());
    }

    public static async Task<LudumDare> GetLudumDare(this Runtime @this, int number)
    {
        if (!Cache.ludumDareList.Any(x => x.number == number))
        {
            using var httpClient = @this.CreateHttpClient();
            var result = await httpClient.Get($"node2/walk/1/events/ludum-dare/{number}");
            Cache.ludumDareList.Add(new()
            {
                id = result.TryGet(x => x.node_id),
                number = number
            });
        }
        return Cache.ludumDareList.Single(x => x.number == number);
    }

    public static async Task<LD_Event> GetEvent(this LudumDare @this)
    {
        return await Cache.events.Get(@this.id);
    }

    public static async Task<List<LD_Game>> GetGames(this LD_Event @this)
    {
        return await Cache.games.Get(@this.feeds.Select(x => x.id).Distinct());
    }

    public static async Task<List<LD_User>> GetUsers(this List<LD_Game> @this)
    {
        return await Cache.users.Get(@this.Select(x => x.author).Distinct());
    }

    public static void ClearGrades(this List<LD_Game> @this)
    {
        foreach (var game in @this)
            game.rating = new();
    }

    public static void ClearComments(this List<LD_Game> @this)
    {
        foreach (var game in @this)
            game.userComments = 0;
    }

    public static void ClearSettings(this List<LD_User> @this)
    {
        foreach (var user in @this)
            user.settings = null;
    }

    public static void FillUsers(this List<LD_Game> @this, List<LD_User> users)
    {
        foreach (var game in @this)
            game.user = users.FirstOrDefault(x => x.id == game.author);
    }

    public static void FillGrades(this List<LD_Game> @this, List<LD_Grade> grades)
    {
        foreach (var grade in grades)
        {
            var game = @this.FirstOrDefault(x => x.id == grade.id);
            if (game is null)
                continue;

            switch (grade.name)
            {
                case "grade-01": game.rating.overall = grade; break;
                case "grade-02": game.rating.fun = grade; break;
                case "grade-03": game.rating.innovation = grade; break;
                case "grade-04": game.rating.theme = grade; break;
                case "grade-05": game.rating.graphics = grade; break;
                case "grade-06": game.rating.audio = grade; break;
                case "grade-07": game.rating.humor = grade; break;
                case "grade-08": game.rating.mood = grade; break;
                default: break;
            }
        }
    }

    public static void FillComments(this List<LD_Game> @this, List<LD_Comment> comments)
    {
        foreach (var game in @this)
            game.userComments = comments.Count(x => x.node == game.id);
    }

    public static async Task<ApiResult> AddGrade(this HttpContext @this, LD_Grade grade)
    {
        using var httpClient = @this.CreateHttpClient();
        return await httpClient.Get($"grade/add/{grade.id}/{grade.name}/{grade.value}");
    }

    public static async Task<ApiResult> RemoveGrade(this HttpContext @this, LD_Grade grade)
    {
        using var httpClient = @this.CreateHttpClient();
        return await httpClient.Get($"grade/remove/{grade.id}/{grade.name}");
    }

    public static async Task<List<LD_Grade>> GetMyGradesOf(this HttpContext @this, LudumDare ludumDare)
    {
        using var httpClient = @this.CreateHttpClient();
        return (await httpClient.Get($"grade/getallmy/{ludumDare.id}"))
            .TryGet(x => x.grade as List<object>, new())
            .Select(x => x.As<LD_Grade>())
            .ToList();
    }

    public static async Task<List<LD_Comment>> GetMyCommentsOf(this HttpContext @this, LudumDare ludumDare)
    {
        using var httpClient = @this.CreateHttpClient();
        var idList = (await httpClient.Get($"comment/getmylistbyparentnode/{ludumDare.id}"))
            .TryGet(x => x.comment as List<object>, new())
            .As<List<long>>();

        var comments = new List<LD_Comment>();
        while (idList.Count > 0)
        {
            var _ids = string.Join('+', idList.Take(Constants.HALF_LIMIT));
            idList.RemoveRange(0, Math.Min(idList.Count, Constants.HALF_LIMIT));

            comments.AddRange(
                (await httpClient.Get($"comment/get/{_ids}"))
                    .TryGet(x => x.comment as List<object>, new())
                    .Select(x => x.As<LD_Comment>())
            );
        }
        return comments;
    }

    public static async Task<T> GetSettings<T>(this HttpContext @this, Func<LD_User.Settings, T> callback)
    {
        var user = await @this.GetUser();
        if (user is null)
        {
            var settings = new LD_User.Settings();
            return callback(settings);
        }
        user.LoadSettings();
        var result = callback(user.settings);
        user.settings = null;
        return result;
    }

    public static async Task<IResult> ModifySettings(this HttpContext @this, Action<LD_User.Settings> callback)
    {
        var user = await @this.GetUser();
        if (user is null)
            return Results.NotFound();
        user.LoadSettings();
        callback(user.settings);
        user.SaveAndUnloadSettings();
        return Results.Ok();
    }

    public static async Task<IResult> ModifyFavoriteGames(this HttpContext @this, Action<List<long>, long> callback)
    {
        var gameId = @this.GetFormLong("gameId");
        if (gameId == 0)
            return Results.NotFound();
        return await ModifySettings(@this, x => callback(x.favoriteGameIds, gameId));
    }

    public static T TryGet<T>(this ApiResult @this, Func<dynamic, T> callback, T defaultValue = default)
    {
        try
        {
            if (@this.ok)
                return callback(@this.data);
        }
        catch { }
        return defaultValue;
    }

    public static List<string> GetCookies(this HttpResponseMessage @this)
    {
        var cookies = new List<string>();
        if (@this.Headers.TryGetValues("Set-Cookie", out var values))
            foreach (var value in values)
                cookies.Add(value);
        return cookies;
    }
}