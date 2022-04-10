using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace LudumDareTools;

public static class Utils
{
    public const string API_BASE_URL = "https://api.ldjam.com/vx/";
    public const string TOKEN_PREFIX = "SIDS=";

    public readonly static LudumDareNodeCollection<Game> games = new();
    public readonly static LudumDareNodeCollection<User> users = new();

    static JsonSerializerOptions jsonOptions;

    public static JsonSerializerOptions GetJsonOptions()
    {
        if (jsonOptions is null)
        {
            jsonOptions = new()
            {
                IncludeFields = true,
                WriteIndented = true
            };
        }
        return jsonOptions;
    }

    static JsonSettings jsonSettings;

    public static JsonSettings GetJsonSettings()
    {
        if (jsonSettings == null)
        {
            jsonSettings = new JsonSettings
            {
                Formatting = JsonFormatting.Indented
            };
        }
        return jsonSettings;
    }

    public static HttpClient CreateHttpClient(string token = null)
    {
        var httpClient = new HttpClient { BaseAddress = new(API_BASE_URL) };

        if (token is not null)
            httpClient.DefaultRequestHeaders.Add("Cookie", TOKEN_PREFIX + token);

        return httpClient;
    }

    public static async Task<(dynamic data, HttpRequestException ex)> Get(this HttpClient @this, string requestUri)
    {
        try
        {
            string json = await @this.GetStringAsync(requestUri);
            var result = Json.DeserializeObject<ExpandoObject>(json, GetJsonSettings());
            return (result, null);
        }
        catch (HttpRequestException ex)
        {
            return (null, ex);
        }
    }

    public static async Task<(HttpResponseMessage message, string[] cookies)> Post(this HttpClient @this, string url, params (string key, object value)[] formData)
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
        var response = await @this.PostAsync(url, content);

        string[] cookies;
        if (response.Headers.TryGetValues("Set-Cookie", out var _cookies))
            cookies = _cookies.ToArray();
        else
            cookies = Array.Empty<string>();

        return (response, cookies);
    }

    public static T As<T>(this object @this)
    {
        var json = Json.SerializeObject(@this, GetJsonSettings());
        return Json.DeserializeObject<T>(json, GetJsonSettings());
    }

    public static string ReadAllText(this FileInfo @this)
    {
        return File.ReadAllText(@this.FullName);
    }

    public static void WriteAllText(this FileInfo @this, string contents)
    {
        File.WriteAllText(@this.FullName, contents);
    }

    public static async Task<long> GetLD(int number)
    {
        using var httpClient = CreateHttpClient();
        var (data, _) = await httpClient.Get($"node2/walk/1/events/ludum-dare/{number}");
        return data is null ? 0 : data.node_id;
    }

    public static async Task<List<Game>> GetRatings(int number, string token)
    {
        using var httpClient = CreateHttpClient(token);

        long ld = await GetLD(number);

        var (data, ex) = await httpClient.Get($"grade/getallmy/{ld}");

        if (ex is not null)
            return null;

        if (data is null)
            return new();

        List<object> dataGrade = data.grade;
        List<Grade> grades = dataGrade.Select(x => x.As<Grade>()).ToList();

        var games = new List<Game>();

        foreach (var grade in grades)
        {
            var game = games.FirstOrDefault(x => x.id == grade.id);
            if (game is null)
            {
                game = await Utils.games.Get(grade.id);
                if (game is null)
                    continue;
                games.Add(game);
            }

            switch (grade.name)
            {
                case "grade-01": game.rating.overall = grade.value; break;
                case "grade-02": game.rating.fun = grade.value; break;
                case "grade-03": game.rating.innovation = grade.value; break;
                case "grade-04": game.rating.theme = grade.value; break;
                case "grade-05": game.rating.graphics = grade.value; break;
                case "grade-06": game.rating.audio = grade.value; break;
                case "grade-07": game.rating.humor = grade.value; break;
                case "grade-08": game.rating.mood = grade.value; break;
                default: break;
            }
        }

        return games;
    }
}