using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class Utils
{
    public const string API_BASE_URL = "https://api.ldjam.com/vx/";
    public const string TOKEN_PREFIX = "SIDS=";

    public static List<Node> nodes;
    public static List<User> users;

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

    static JsonSerializerSettings jsonSettings;

    public static JsonSerializerSettings GetJsonSettings()
    {
        if (jsonSettings == null)
        {
            jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
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
            var result = JsonConvert.DeserializeObject<ExpandoObject>(json, GetJsonSettings());
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
        var json = JsonConvert.SerializeObject(@this, GetJsonSettings());
        return JsonConvert.DeserializeObject<T>(json, GetJsonSettings());
    }

    public static void Load<T>(ref List<T> list, string path)
    {
        if (list is not null)
            return;

        list = new();

        path = Path.Combine("data", path);

        Directory.CreateDirectory(path);

        foreach (var file in Directory.EnumerateFiles(path, "*.json"))
        {
            var json = File.ReadAllText(file);
            var item = JsonConvert.DeserializeObject<T>(json, GetJsonSettings());
            list.Add(item);
        }
    }

    public static void Save<T>(List<T> list, string path, T item, object name)
    {
        path = Path.Combine("data", path);

        Directory.CreateDirectory(path);

        var json = JsonConvert.SerializeObject(item, GetJsonSettings());
        File.WriteAllText(Path.Combine(path, $"{name}.json"), json);

        list.Add(item);
    }

    public static void Delete<T>(List<T> list, string path, Predicate<T> predicate, object name)
    {
        list.RemoveAll(predicate);

        path = Path.Combine("data", path, $"{name}.json");
        if (File.Exists(path))
            File.Delete(path);
    }

    public static string ToMD5(this string @this)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(@this));
        var result = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
            result.Append(hash[i].ToString("X2"));
        return result.ToString().ToLower();
    }

    public static async Task<List<Node>> GetRatings(int number, string token)
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

        var nodes = new List<Node>();

        foreach (var grade in grades)
        {
            var node = nodes.FirstOrDefault(x => x.id == grade.id);
            if (node is null)
            {
                node = await GetNode(grade.id, false);
                nodes.Add(node);
            }

            switch (grade.name)
            {
                case "grade-01": node.rating.overall = grade.value; break;
                case "grade-02": node.rating.fun = grade.value; break;
                case "grade-03": node.rating.innovation = grade.value; break;
                case "grade-04": node.rating.theme = grade.value; break;
                case "grade-05": node.rating.graphics = grade.value; break;
                case "grade-06": node.rating.audio = grade.value; break;
                case "grade-07": node.rating.humor = grade.value; break;
                case "grade-08": node.rating.mood = grade.value; break;
                default: break;
            }
        }

        return nodes;
    }

    public static async Task<long> GetLD(int number)
    {
        using var httpClient = CreateHttpClient();
        var (data, _) = await httpClient.Get($"node2/walk/1/events/ludum-dare/{number}");
        return data is null ? 0 : data.node_id;
    }

    public static async Task<Node> GetNode(long id, bool force)
    {
        Load(ref nodes, "nodes");

        if (nodes.Any(x => x.id == id))
        {
            if (force)
            {
                nodes.RemoveAll(x => x.id == id);
                goto ForceSection;
            }

            var existingNode = nodes.FirstOrDefault(x => x.id == id);

            existingNode.user = await GetUser(existingNode.author);

            return existingNode;
        }

    ForceSection:

        using var httpClient = CreateHttpClient();

        var data = (await httpClient.Get($"node/get/{id}")).data.node[0];

        var node = (data as object).As<Node>();

        try { node.cover = data.meta.cover; } catch { }
        
        Save(nodes, "nodes", node, node.id);

        node.user = await GetUser(node.author);

        return node;
    }

    public static async Task<User> GetUser(long id)
    {
        Load(ref users, "users");

        if (users.Any(x => x.id == id))
            return users.FirstOrDefault(x => x.id == id);

        using var httpClient = CreateHttpClient();

        var user = ((await httpClient.Get($"node/get/{id}")).data.node[0] as object).As<User>();

        Save(users, "users", user, user.id);

        return user;
    }
}