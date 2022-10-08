namespace LudumDareTools;

using System.Dynamic;
using System.Net.Http;
using System.Text;

public static class ExtensionMethods
{
    public static async Task<(dynamic data, HttpRequestException ex)> Get(this HttpClient @this, string url)
    {
        try
        {
            Console.WriteLine($"GET {Constants.API_BASE_URL}{url}");
            string json = await @this.GetStringAsync(url);
            var result = Json.DeserializeObject<ExpandoObject>(json, Utils.GetJsonSettings());
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

        Console.WriteLine($"POST {Constants.API_BASE_URL}{url}");
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
        var json = Json.SerializeObject(@this, Utils.GetJsonSettings());
        return Json.DeserializeObject<T>(json, Utils.GetJsonSettings());
    }

    public static bool Has(this string @this, string value)
    {
        return @this.Contains(value, StringComparison.InvariantCultureIgnoreCase);
    }
}